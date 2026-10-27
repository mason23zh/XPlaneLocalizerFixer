using Microsoft.Extensions.Options;
using Moq;
using XPlaneLocalizerFixer.Configuration;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories;
using XPlaneLocalizerFixer.Services;

namespace XPlaneLocalizerFixer.Tests.Services;

/// <summary>
///     Integration tests for the complete localizer correction workflow
///     using real parsers, matchers, and correction services.
/// </summary>
public sealed class LocalizerCorrectionProcessorIntegrationTest
{
    [Fact]
    public void Process_WhenIlsHasValidRunwayAndNavAids_ReturnsSuccessfulCorrection()
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

        // Controlled runway geometry follows approximately the
        // 125.679-degree true course, with a small lateral offset.
        // This is a test fixture, not the actual SBGL apt.dat runway.
        var airports = new AptDatParser().Parse(
        [
            "1 26 0 0 SBGL Galeao Intl",
            "100 45 1 0 0.25 1 2 0 " +
            "15 -22.821193645 -43.250325712 0 0 0 0 0 0 " +
            "33 -22.836929585 -43.226547774 0 0 0 0 0 0"
        ]);

        var airportRepository = new Mock<IAirportRepository>();
        var navDataRepository = new Mock<INavDataRepository>();

        airportRepository
            .Setup(repository => repository.GetAirports())
            .Returns(airports);

        navDataRepository
            .Setup(repository => repository.GetNavData())
            .Returns(navData);

        var processor = new LocalizerCorrectionProcessor(
            airportRepository.Object,
            navDataRepository.Object,
            new LocalizerNavAidMatcher(),
            new LocalizerRunwayMatcher(),
            new LocalizerCorrector(
                Options.Create(new LocalizerCorrectionOptions
                {
                    MaxLateralDeviationMeters = 50.0,
                    MaxHeadingDeviationDegrees = 1.0
                })
            ));

        // Act
        var results = processor.Process();

        // Assert
        var result = Assert.Single(results);

        Assert.True(result.Correction.IsCorrected);
        Assert.Null(result.Correction.SkipReason);

        Assert.NotNull(result.Correction.CorrectedLatitude);
        Assert.NotNull(result.Correction.CorrectedLongitude);
        Assert.NotNull(result.Correction.CorrectedEncodedCourse);

        // Verify that the correction remains within the safety limit.
        Assert.NotNull(result.Correction.LateralDisplacementMeters);
        Assert.InRange(
            Math.Abs(result.Correction.LateralDisplacementMeters.Value),
            0,
            50);

        // All navigation aids must reference their original parsed objects.
        Assert.Same(navData.Localizers[0], result.NavAids.Localizer);
        Assert.Same(navData.GlideSlopes[0], result.NavAids.GlideSlope);
        Assert.Same(navData.Dmes[0], result.NavAids.Dme);

        // GS and DME records must remain completely unchanged.
        Assert.Equal(gsLine, result.NavAids.GlideSlope!.RawLine);
        Assert.Equal(dmeLine, result.NavAids.Dme!.RawLine);

        // The original localizer must not be modified either.
        Assert.Equal(locLine, result.Correction.Localizer.RawLine);
    }

    [Fact]
    public void Process_WhenIlsIsIntentionallyOffset_SkipsCorrection()
    {
        // Arrange
        const string locLine =
            "4 38.966627778 121.547938889 125 10910 18 36092.396 IZF ZYTL ZY 10 ILS-cat-I";

        const string gsLine =
            "6 38.965597222 121.525325000 125 10910 18 330092.396 IZF ZYTL ZY 10 GS";

        const string airportHeader =
            "1 26 0 0 ZYTL Dalian Zhoushuizi Intl";

        // Actual runway endpoints from ZYTL apt.dat.
        const string runwayLine =
            "100 45.00 31 827 0.25 1 3 0 " +
            "10 38.9671338 121.5195924 200 60 7 1 0 2 " +
            "28 38.9642194 121.5574804 300 61 7 1 0 2";

        var navData = new EarthNavDatParser().Parse(
        [
            locLine,
            gsLine
        ]);

        var airports = new AptDatParser().Parse(
        [
            airportHeader,
            runwayLine
        ]);

        var localizer = Assert.Single(navData.Localizers);
        var glideSlope = Assert.Single(navData.GlideSlopes);

        // Use the real matchers and correction algorithm.
        var navAidMatcher = new LocalizerNavAidMatcher();
        var runwayMatcher = new LocalizerRunwayMatcher();
        var corrector = new LocalizerCorrector(
            Options.Create(new LocalizerCorrectionOptions
            {
                MaxLateralDeviationMeters = 50.0,
                MaxHeadingDeviationDegrees = 1.0
            })
        );

        var navAidMatch = navAidMatcher.Match(localizer, navData);

        Assert.Same(glideSlope, navAidMatch.GlideSlope);
        Assert.True(navAidMatch.IsEligibleForCorrection);

        var runwayMatch = runwayMatcher.Match(localizer, airports);

        Assert.NotNull(runwayMatch);

        // Verify that the real correction algorithm rejects the offset ILS.
        var correction = corrector.Correct(runwayMatch);

        Assert.False(correction.IsCorrected);
        Assert.NotNull(correction.SkipReason);

        // Verify that the complete processor excludes the rejected correction.
        var airportRepository = new Mock<IAirportRepository>();
        var navDataRepository = new Mock<INavDataRepository>();

        airportRepository
            .Setup(repository => repository.GetAirports())
            .Returns(airports);

        navDataRepository
            .Setup(repository => repository.GetNavData())
            .Returns(navData);

        var processor = new LocalizerCorrectionProcessor(
            airportRepository.Object,
            navDataRepository.Object,
            navAidMatcher,
            runwayMatcher,
            corrector);

        var results = processor.Process();

        Assert.Empty(results);

        // Confirm that the original navigation records remain unchanged.
        Assert.Equal(locLine, localizer.RawLine);
        Assert.Equal(gsLine, glideSlope.RawLine);
    }
}