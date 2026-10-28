using Moq;
using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories;
using XPlaneLocalizerFixer.Services;

namespace XPlaneLocalizerFixer.Tests.Services;

public class LocalizerCorrectionProcessorTest
{
    [Fact]
    public void Process_WhenGlideSlopeIsMissing_DoesNotCorrectLocalizer()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        var navData = new EarthNavDatParser().Parse([locLine]);
        var localizer = Assert.Single(navData.Localizers);

        var airportRepository = new Mock<IAirportRepository>();
        var navDataRepository = new Mock<INavDataRepository>();
        var navAidMatcher = new Mock<ILocalizerNavAidMatcher>();
        var runwayMatcher = new Mock<ILocalizerRunwayMatcher>();
        var corrector = new Mock<ILocalizerCorrector>();

        airportRepository
            .Setup(repository => repository.GetAirports())
            .Returns([]);

        navDataRepository
            .Setup(repository => repository.GetNavData())
            .Returns(navData);

        navAidMatcher
            .Setup(matcher => matcher.Match(localizer, navData))
            .Returns(new LocalizerNavAidMatch
            {
                Localizer = localizer,
                GlideSlope = null,
                Dme = null
            });

        var processor = new LocalizerCorrectionProcessor(
            airportRepository.Object,
            navDataRepository.Object,
            navAidMatcher.Object,
            runwayMatcher.Object,
            corrector.Object);

        // Act
        var results = processor.Process(new HashSet<string>());

        // Assert
        Assert.Empty(results);

        navAidMatcher.Verify(
            matcher => matcher.Match(localizer, navData),
            Times.Once);

        runwayMatcher.Verify(
            matcher => matcher.Match(
                It.IsAny<Localizer>(),
                It.IsAny<IEnumerable<Airport>>()),
            Times.Never);

        corrector.Verify(
            service => service.Correct(
                It.IsAny<LocalizerRunwayMatch>()),
            Times.Never);
    }

    [Fact]
    public void Process_WhenLocalizerIsRow5_SkipsWithoutMatching()
    {
        // Arrange
        const string locLine =
            "5 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 LOC";

        var navData = new EarthNavDatParser().Parse([locLine]);

        var airportRepository = new Mock<IAirportRepository>();
        var navDataRepository = new Mock<INavDataRepository>();
        var navAidMatcher = new Mock<ILocalizerNavAidMatcher>();
        var runwayMatcher = new Mock<ILocalizerRunwayMatcher>();
        var corrector = new Mock<ILocalizerCorrector>();

        airportRepository
            .Setup(repository => repository.GetAirports())
            .Returns([]);

        navDataRepository
            .Setup(repository => repository.GetNavData())
            .Returns(navData);

        var processor = new LocalizerCorrectionProcessor(
            airportRepository.Object,
            navDataRepository.Object,
            navAidMatcher.Object,
            runwayMatcher.Object,
            corrector.Object);

        // Act
        var results = processor.Process(new HashSet<string>());

        // Assert
        Assert.Empty(results);

        // Standalone localizers must never reach the matching services.
        navAidMatcher.Verify(
            matcher => matcher.Match(
                It.IsAny<Localizer>(),
                It.IsAny<NavData>()),
            Times.Never);

        runwayMatcher.Verify(
            matcher => matcher.Match(
                It.IsAny<Localizer>(),
                It.IsAny<IEnumerable<Airport>>()),
            Times.Never);

        corrector.Verify(
            service => service.Correct(
                It.IsAny<LocalizerRunwayMatch>()),
            Times.Never);
    }

    [Fact]
    public void Process_WhenRunwayIsMissing_DoesNotCorrectLocalizer()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        const string gsLine =
            "6 -22.815583333 -43.261000000 17 11050 18 300125.679 IJB SBGL SB 15 GS";

        var navData = new EarthNavDatParser().Parse(
        [
            locLine,
            gsLine
        ]);

        var localizer = Assert.Single(navData.Localizers);
        var glideSlope = Assert.Single(navData.GlideSlopes);

        var airportRepository = new Mock<IAirportRepository>();
        var navDataRepository = new Mock<INavDataRepository>();
        var navAidMatcher = new Mock<ILocalizerNavAidMatcher>();
        var runwayMatcher = new Mock<ILocalizerRunwayMatcher>();
        var corrector = new Mock<ILocalizerCorrector>();

        airportRepository
            .Setup(repository => repository.GetAirports())
            .Returns([]);

        navDataRepository
            .Setup(repository => repository.GetNavData())
            .Returns(navData);

        navAidMatcher
            .Setup(matcher => matcher.Match(localizer, navData))
            .Returns(new LocalizerNavAidMatch
            {
                Localizer = localizer,
                GlideSlope = glideSlope
            });

        // No matching runway exists.
        runwayMatcher
            .Setup(matcher => matcher.Match(
                localizer,
                It.IsAny<IEnumerable<Airport>>()))
            .Returns((LocalizerRunwayMatch?)null);

        var processor = new LocalizerCorrectionProcessor(
            airportRepository.Object,
            navDataRepository.Object,
            navAidMatcher.Object,
            runwayMatcher.Object,
            corrector.Object);

        // Act
        var results = processor.Process(new HashSet<string>());

        // Assert
        Assert.Empty(results);

        navAidMatcher.Verify(
            matcher => matcher.Match(localizer, navData),
            Times.Once);

        runwayMatcher.Verify(
            matcher => matcher.Match(
                localizer,
                It.IsAny<IEnumerable<Airport>>()),
            Times.Once);

        // The correction algorithm must never run without a matching runway.
        corrector.Verify(
            service => service.Correct(
                It.IsAny<LocalizerRunwayMatch>()),
            Times.Never);
    }

    [Fact]
    public void Process_WhenCorrectionIsRejected_DoesNotIncludeResult()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        const string gsLine =
            "6 -22.815583333 -43.261000000 17 11050 18 300125.679 IJB SBGL SB 15 GS";

        var navData = new EarthNavDatParser().Parse(
        [
            locLine,
            gsLine
        ]);

        var localizer = Assert.Single(navData.Localizers);
        var glideSlope = Assert.Single(navData.GlideSlopes);

        var airportRepository = new Mock<IAirportRepository>();
        var navDataRepository = new Mock<INavDataRepository>();
        var navAidMatcher = new Mock<ILocalizerNavAidMatcher>();
        var runwayMatcher = new Mock<ILocalizerRunwayMatcher>();
        var corrector = new Mock<ILocalizerCorrector>();

        // Repositories provide the source data.
        airportRepository
            .Setup(repository => repository.GetAirports())
            .Returns([]);

        navDataRepository
            .Setup(repository => repository.GetNavData())
            .Returns(navData);

        navAidMatcher
            .Setup(matcher => matcher.Match(localizer, navData))
            .Returns(new LocalizerNavAidMatch
            {
                Localizer = localizer,
                GlideSlope = glideSlope
            });

        // Use the actual runway matcher to create a valid match object.
        const string airportHeader = "1 26 0 0 SBGL Galeao Intl";

        const string runwayLine =
            "100 45 1 0 0.25 1 2 0 15 -22.8160000 -43.2600000 0 0 0 0 0 0 33 -22.8300000 -43.2370000 0 0 0 0 0 0";

        var airports = new AptDatParser().Parse(
        [
            airportHeader,
            runwayLine
        ]);

        var runwayMatch = new LocalizerRunwayMatcher()
            .Match(localizer, airports);

        Assert.NotNull(runwayMatch);

        runwayMatcher
            .Setup(matcher => matcher.Match(
                localizer,
                It.IsAny<IEnumerable<Airport>>()))
            .Returns(runwayMatch);

        // Simulate a correction rejected by the existing safety checks.
        corrector
            .Setup(service => service.Correct(runwayMatch))
            .Returns(new LocalizerCorrectionResult
            {
                Localizer = localizer,
                IsCorrected = false,
                SkipReason = "Lateral displacement exceeds 50 meters."
            });

        var processor = new LocalizerCorrectionProcessor(
            airportRepository.Object,
            navDataRepository.Object,
            navAidMatcher.Object,
            runwayMatcher.Object,
            corrector.Object);

        // Act
        var results = processor.Process(new HashSet<string>());

        // Assert
        Assert.Empty(results);

        corrector.Verify(
            service => service.Correct(runwayMatch),
            Times.Once);
    }

    [Fact]
    public void Process_WhenCorrectionSucceeds_ReturnsCorrectionWithOriginalNavAids()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        const string gsLine =
            "6 -22.815583333 -43.261000000 17 11050 18 300125.679 IJB SBGL SB 15 GS";

        const string dmeLine =
            "12 -22.829694444 -43.236472222 23 11050 25 0.000 IJB SBGL SB GALEAO-ANTONIO C JOBIM IN DME-ILS";

        var navData = new EarthNavDatParser().Parse(
        [
            locLine,
            gsLine,
            dmeLine
        ]);

        var localizer = Assert.Single(navData.Localizers);
        var glideSlope = Assert.Single(navData.GlideSlopes);
        var dme = Assert.Single(navData.Dmes);

        var airports = new AptDatParser().Parse(
        [
            "1 26 0 0 SBGL Galeao Intl",
            "100 45 1 0 0.25 1 2 0 15 -22.8160000 -43.2600000 0 0 0 0 0 0 33 -22.8300000 -43.2370000 0 0 0 0 0 0"
        ]);

        var runwayMatch = new LocalizerRunwayMatcher()
            .Match(localizer, airports);

        Assert.NotNull(runwayMatch);

        var expectedCorrection = new LocalizerCorrectionResult
        {
            Localizer = localizer,
            IsCorrected = true,
            CorrectedLatitude = -22.830117480,
            CorrectedLongitude = -43.236977126,
            CorrectedEncodedCourse = 53765.527,
            LateralDisplacementMeters = 0.9
        };

        var airportRepository = new Mock<IAirportRepository>();
        var navDataRepository = new Mock<INavDataRepository>();
        var navAidMatcher = new Mock<ILocalizerNavAidMatcher>();
        var runwayMatcher = new Mock<ILocalizerRunwayMatcher>();
        var corrector = new Mock<ILocalizerCorrector>();

        airportRepository
            .Setup(repository => repository.GetAirports())
            .Returns(airports);

        navDataRepository
            .Setup(repository => repository.GetNavData())
            .Returns(navData);

        var expectedNavAidMatch = new LocalizerNavAidMatch
        {
            Localizer = localizer,
            GlideSlope = glideSlope,
            Dme = dme
        };

        navAidMatcher
            .Setup(matcher => matcher.Match(localizer, navData))
            .Returns(expectedNavAidMatch);

        runwayMatcher
            .Setup(matcher => matcher.Match(
                localizer,
                It.IsAny<IEnumerable<Airport>>()))
            .Returns(runwayMatch);

        corrector
            .Setup(service => service.Correct(runwayMatch))
            .Returns(expectedCorrection);

        var processor = new LocalizerCorrectionProcessor(
            airportRepository.Object,
            navDataRepository.Object,
            navAidMatcher.Object,
            runwayMatcher.Object,
            corrector.Object);

        // Act
        var results = processor.Process(new HashSet<string>());

        // Assert
        var result = Assert.Single(results);

        Assert.Same(expectedCorrection, result.Correction);
        Assert.Same(expectedNavAidMatch, result.NavAids);

        Assert.True(result.Correction.IsCorrected);
        Assert.Equal(-22.830117480, result.Correction.CorrectedLatitude);
        Assert.Equal(-43.236977126, result.Correction.CorrectedLongitude);
        Assert.Equal(53765.527, result.Correction.CorrectedEncodedCourse);

        // Preserve the original GS and DME objects and their source records.
        Assert.Same(glideSlope, result.NavAids.GlideSlope);
        Assert.Same(dme, result.NavAids.Dme);
        Assert.Equal(gsLine, result.NavAids.GlideSlope!.RawLine);
        Assert.Equal(dmeLine, result.NavAids.Dme!.RawLine);

        corrector.Verify(
            service => service.Correct(runwayMatch),
            Times.Once);
    }

    [Fact]
    public void Process_WhenNavAidMatchIsAmbiguous_ContinuesProcessingOtherLocalizers()
    {
        // Arrange
        const string loc15Line =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        const string loc28Line =
            "4 -22.802666667 -43.258500000 28 11150 18 100334.375 ILM SBGL SB 28 ILS-cat-I";

        const string gs28Line =
            "6 -22.791944444 -43.221055556 28 11150 18 300254.375 ILM SBGL SB 28 GS";

        var navData = new EarthNavDatParser().Parse(
        [
            loc15Line,
            loc28Line,
            gs28Line
        ]);

        var loc15 = navData.Localizers[0];
        var loc28 = navData.Localizers[1];
        var gs28 = Assert.Single(navData.GlideSlopes);

        var airports = new AptDatParser().Parse(
        [
            "1 26 0 0 SBGL Galeao Intl",
            "100 45 1 0 0.25 1 2 0 15 -22.8160000 -43.2600000 0 0 0 0 0 0 33 -22.8300000 -43.2370000 0 0 0 0 0 0",
            "100 45 1 0 0.25 1 2 0 10 -22.8000000 -43.2200000 0 0 0 0 0 0 28 -22.8030000 -43.2590000 0 0 0 0 0 0"
        ]);

        var runwayMatch = new LocalizerRunwayMatcher()
            .Match(loc28, airports);

        Assert.NotNull(runwayMatch);

        var expectedCorrection = new LocalizerCorrectionResult
        {
            Localizer = loc28,
            IsCorrected = true,
            CorrectedLatitude = -22.802670000,
            CorrectedLongitude = -43.258510000,
            CorrectedEncodedCourse = 100334.200
        };

        var airportRepository = new Mock<IAirportRepository>();
        var navDataRepository = new Mock<INavDataRepository>();
        var navAidMatcher = new Mock<ILocalizerNavAidMatcher>();
        var runwayMatcher = new Mock<ILocalizerRunwayMatcher>();
        var corrector = new Mock<ILocalizerCorrector>();

        airportRepository
            .Setup(repository => repository.GetAirports())
            .Returns(airports);

        navDataRepository
            .Setup(repository => repository.GetNavData())
            .Returns(navData);

        // Simulate an ambiguous association for RWY 15.
        navAidMatcher
            .Setup(matcher => matcher.Match(loc15, navData))
            .Throws<InvalidOperationException>();

        // RWY 28 has a unique matching glideslope.
        var validMatch = new LocalizerNavAidMatch
        {
            Localizer = loc28,
            GlideSlope = gs28
        };

        navAidMatcher
            .Setup(matcher => matcher.Match(loc28, navData))
            .Returns(validMatch);

        runwayMatcher
            .Setup(matcher => matcher.Match(
                loc28,
                It.IsAny<IEnumerable<Airport>>()))
            .Returns(runwayMatch);

        corrector
            .Setup(service => service.Correct(runwayMatch))
            .Returns(expectedCorrection);

        var processor = new LocalizerCorrectionProcessor(
            airportRepository.Object,
            navDataRepository.Object,
            navAidMatcher.Object,
            runwayMatcher.Object,
            corrector.Object);

        // Act
        var results = processor.Process(new HashSet<string>());

        // Assert
        var result = Assert.Single(results);

        Assert.Same(expectedCorrection, result.Correction);
        Assert.Same(validMatch, result.NavAids);
        Assert.Same(loc28, result.Correction.Localizer);

        // Both localizers were examined despite the first match failing.
        navAidMatcher.Verify(
            matcher => matcher.Match(loc15, navData),
            Times.Once);

        navAidMatcher.Verify(
            matcher => matcher.Match(loc28, navData),
            Times.Once);

        // Only the valid localizer reaches the correction service.
        corrector.Verify(
            service => service.Correct(
                It.IsAny<LocalizerRunwayMatch>()),
            Times.Once);
    }
    
    
    [Fact]
    public void Process_WhenAirportIsExcluded_DoesNotCorrectLocalizer()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        var navData = new EarthNavDatParser().Parse([locLine]);
        var localizer = Assert.Single(navData.Localizers);

        var airportRepository = new Mock<IAirportRepository>();
        var navDataRepository = new Mock<INavDataRepository>();
        var navAidMatcher = new Mock<ILocalizerNavAidMatcher>();
        var runwayMatcher = new Mock<ILocalizerRunwayMatcher>();
        var corrector = new Mock<ILocalizerCorrector>();

        airportRepository
            .Setup(repository => repository.GetAirports())
            .Returns([]);

        navDataRepository
            .Setup(repository => repository.GetNavData())
            .Returns(navData);

        var processor = new LocalizerCorrectionProcessor(
            airportRepository.Object,
            navDataRepository.Object,
            navAidMatcher.Object,
            runwayMatcher.Object,
            corrector.Object);

        IReadOnlySet<string> excludedAirports =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "SBGL"
            };

        // Act
        var results = processor.Process(excludedAirports);

        // Assert
        Assert.Empty(results);

        navAidMatcher.Verify(
            matcher => matcher.Match(
                It.IsAny<Localizer>(),
                It.IsAny<NavData>()),
            Times.Never);

        runwayMatcher.Verify(
            matcher => matcher.Match(
                It.IsAny<Localizer>(),
                It.IsAny<IEnumerable<Airport>>()),
            Times.Never);

        corrector.Verify(
            service => service.Correct(
                It.IsAny<LocalizerRunwayMatch>()),
            Times.Never);
    }

}