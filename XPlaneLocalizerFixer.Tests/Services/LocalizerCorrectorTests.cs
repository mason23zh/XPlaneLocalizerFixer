using XPlaneLocalizerFixer.Geometry;
using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Services;

namespace XPlaneLocalizerFixer.Tests.Services;

public class LocalizerCorrectorTests
{
    [Fact]
    public void Correct_LocalizerWithinTolerance_ProjectsOntoRunwayCenterline()
    {
        // A northbound runway with a localizer 10 metres east
        // of the extended runway centerline.
        var runway = new Runway
        {
            Identifier = "36",
            StartPosition = new GeoCoordinate(0, 0),
            EndPosition = new GeoCoordinate(0.01, 0)
        };

        var centerlinePosition = Wgs84Geodesic.Direct(
            runway.StartPosition,
            180.0,
            100.0);

        Assert.NotNull(centerlinePosition);

        var position = Wgs84Geodesic.Direct(
            centerlinePosition.Value,
            90.0,
            10.0);

        Assert.NotNull(position);

        var localizer = new Localizer
        {
            RowCode = 4,
            Position = new GeoCoordinate(
                position.Value.Latitude,
                position.Value.Longitude),
            ElevationFeet = 100,
            Frequency = 11050,
            RangeNm = 18,
            Identifier = "ITST",
            AirportIcao = "TEST",
            RegionCode = "XX",
            RunwayIdentifier = "36",
            Name = "ILS-cat-I",
            EncodedCourse = 36000,
            RawLine = "Synthetic test localizer"
        };

        var match = new LocalizerRunwayMatch(
            localizer,
            null!,
            runway);

        var corrector = new LocalizerCorrector();

        var result = corrector.Correct(match);

        Assert.True(result.IsCorrected, result.SkipReason);
        Assert.NotNull(result.LateralDisplacementMeters);
        Assert.InRange(
            result.LateralDisplacementMeters.Value,
            9.9,
            10.1);

        Assert.NotNull(result.CorrectedLatitude);
        Assert.NotNull(result.CorrectedLongitude);

        var corrected = new GeoCoordinate(
            result.CorrectedLatitude.Value,
            result.CorrectedLongitude.Value);

        Assert.InRange(
            Math.Abs(corrected.Longitude),
            0.0,
            1e-8);

        // Verify the corrected position is approximately 100 metres
        // south of the runway threshold using WGS84 distances.
        var correctedDistance = Wgs84Geodesic.Inverse(
            runway.StartPosition,
            corrected);

        Assert.NotNull(correctedDistance);

        Assert.InRange(
            correctedDistance.Value.Distance,
            99.999,
            100.001);

        // The original parsed localizer must remain unchanged.
        Assert.Equal(
            localizer.Position.Latitude,
            position.Value.Latitude);

        Assert.Equal(
            localizer.Position.Longitude,
            position.Value.Longitude);
    }

    [Fact]
    public void Correct_LateralDisplacementExceeds50Meters_SkipsCorrection()
    {
        var runway = new Runway
        {
            Identifier = "36",
            StartPosition = new GeoCoordinate(0, 0),
            EndPosition = new GeoCoordinate(0.01, 0)
        };

        var projection = new GnomonicProjection(0, 0);

        // Position the localizer 60 metres east of the centerline.
        var position = projection.Unproject(new Vector2D(60, -100));

        var localizer = new Localizer
        {
            RowCode = 4,
            Position = new GeoCoordinate(position.Latitude, position.Longitude),
            ElevationFeet = 100,
            Frequency = 11050,
            RangeNm = 18,
            Identifier = "ITST",
            AirportIcao = "TEST",
            RegionCode = "XX",
            RunwayIdentifier = "36",
            Name = "ILS-cat-I",
            EncodedCourse = 36000,
            RawLine = "Synthetic test localizer"
        };

        var match = new LocalizerRunwayMatch(localizer, null!, runway);

        var corrector = new LocalizerCorrector();

        var result = corrector.Correct(match);

        Assert.False(result.IsCorrected);
        Assert.NotNull(result.SkipReason);
        Assert.Contains("50", result.SkipReason);
        Assert.True(result.LateralDisplacementMeters!.Value > 50.0);

        Assert.Null(result.CorrectedLatitude);
        Assert.Null(result.CorrectedLongitude);
        Assert.Null(result.CorrectedEncodedCourse);
    }

    [Fact]
    public void Correct_HeadingDifferenceExceedsOneDegree_SkipsCorrection()
    {
        var runway = new Runway
        {
            Identifier = "36",
            StartPosition = new GeoCoordinate(0, 0),
            EndPosition = new GeoCoordinate(0.01, 0)
        };

        var projection = new GnomonicProjection(0, 0);

        var position = projection.Unproject(new Vector2D(10, -100));

        var localizer = new Localizer
        {
            RowCode = 4,
            Position = new GeoCoordinate(position.Latitude, position.Longitude),
            ElevationFeet = 100,
            Frequency = 11050,
            RangeNm = 18,
            Identifier = "ITST",
            AirportIcao = "TEST",
            RegionCode = "XX",
            RunwayIdentifier = "36",
            Name = "ILS-cat-I",

            // Magnetic prefix = 100 * 360; true bearing = 3°.
            EncodedCourse = 36003,

            RawLine = "Synthetic offset localizer"
        };

        var match = new LocalizerRunwayMatch(localizer, null!, runway);

        var corrector = new LocalizerCorrector();

        var result = corrector.Correct(match);

        Assert.False(result.IsCorrected);
        Assert.NotNull(result.SkipReason);
        Assert.Contains("heading", result.SkipReason, StringComparison.OrdinalIgnoreCase);

        Assert.Null(result.CorrectedLatitude);
        Assert.Null(result.CorrectedLongitude);
        Assert.Null(result.CorrectedEncodedCourse);
        Assert.Null(result.LateralDisplacementMeters);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(0.999)]
    [InlineData(359.001)]
    public void Correct_HeadingWithinTolerance_PreservesMagneticCourse(
        double trueBearing)
    {
        var runway = new Runway
        {
            Identifier = "36",
            StartPosition = new GeoCoordinate(0, 0),
            EndPosition = new GeoCoordinate(0.01, 0)
        };

        var projection = new GnomonicProjection(0, 0);
        var position = projection.Unproject(new Vector2D(10, -100));

        const int magneticFrontCourse = 164;

        var localizer = new Localizer
        {
            RowCode = 4,
            Position = new GeoCoordinate(
                position.Latitude,
                position.Longitude),
            ElevationFeet = 100,
            Frequency = 11050,
            RangeNm = 18,
            Identifier = "ITST",
            AirportIcao = "TEST",
            RegionCode = "XX",
            RunwayIdentifier = "36",
            Name = "ILS-cat-I",

            EncodedCourse = magneticFrontCourse * 360 + trueBearing,

            RawLine = "Synthetic test localizer"
        };

        var match = new LocalizerRunwayMatch(localizer, null!, runway);
        var corrector = new LocalizerCorrector();

        var result = corrector.Correct(match);

        Assert.True(result.IsCorrected, result.SkipReason);
        Assert.NotNull(result.CorrectedEncodedCourse);

        var correctedCourse = result.CorrectedEncodedCourse.Value;

        Assert.Equal(
            magneticFrontCourse,
            (int)Math.Floor(correctedCourse / 360.0));

        Assert.Equal(0.0, correctedCourse % 360.0, 6);

        Assert.NotNull(result.CorrectedLatitude);
        Assert.InRange(
            result.LateralDisplacementMeters!.Value,
            9.9,
            10.1);
    }

    [Fact]
    public void Correct_SBGLRunway15_AlignsLocalizerWithCenterline()
    {
        // Real SBGL runway 15/33 coordinates from apt.dat.
        var runway = new Runway
        {
            Identifier = "15",
            StartPosition = new GeoCoordinate(
                -22.8124163, -43.2637261),
            EndPosition = new GeoCoordinate(
                -22.8291396, -43.2384552)
        };

        // Original SBGL runway 15 ILS from earth_nav.dat.
        var localizer = new Localizer
        {
            RowCode = 4,
            Position = new GeoCoordinate(
                -22.830111111, -43.236972222),
            ElevationFeet = 17,
            Frequency = 11050,
            RangeNm = 18,
            Identifier = "IJB",
            AirportIcao = "SBGL",
            RegionCode = "SB",
            RunwayIdentifier = "15",
            Name = "ILS-cat-I",
            EncodedCourse = 53765.679,
            RawLine = "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I"
        };

        var match = new LocalizerRunwayMatch(
            localizer,
            null!,
            runway);

        var corrector = new LocalizerCorrector();

        var result = corrector.Correct(match);

        Assert.True(result.IsCorrected, result.SkipReason);


        // Expected WGS84 correction, verified against the previous
        // successful SBGL RWY 15 in-game alignment.
        Assert.NotNull(result.CorrectedLatitude);
        Assert.NotNull(result.CorrectedLongitude);
        Assert.NotNull(result.CorrectedEncodedCourse);
        Assert.NotNull(result.LateralDisplacementMeters);

        Assert.InRange(
            Math.Abs(result.CorrectedLongitude.Value - -43.236977125),
            0.0,
            1e-8);

        Assert.InRange(
            Math.Abs(result.CorrectedLatitude.Value - -22.830117481),
            0.0,
            1e-8);

        Assert.Equal(
            53765.527,
            result.CorrectedEncodedCourse.Value,
            3);

        // The original localizer was approximately 0.87 m
        // laterally displaced from the WGS84 runway centerline.
        Assert.InRange(
            result.LateralDisplacementMeters.Value,
            0.86,
            0.88);


        // The original localizer must remain unchanged.
        Assert.Equal(
            new GeoCoordinate(-22.830111111, -43.236972222),
            localizer.Position);

        Assert.Equal(53765.679, localizer.EncodedCourse);
        Assert.Equal(17, localizer.ElevationFeet);
    }


    [Fact]
    public void Correct_VHHHRunway25L_AlignsLocalizerWithCenterline()
    {
        // Real VHHH runway 25L/07R coordinates from apt.dat.
        var runway = new Runway
        {
            Identifier = "25L",
            StartPosition = new GeoCoordinate(
                22.3074327, 113.9328435),
            EndPosition = new GeoCoordinate(
                22.2961895, 113.8979789)
        };

        // Original VHHH runway 25L ILS from earth_nav.dat.
        var localizer = new Localizer
        {
            RowCode = 4,
            Position = new GeoCoordinate(
                22.295272222, 113.895083333),
            ElevationFeet = 20,
            Frequency = 11090,
            RangeNm = 18,
            Identifier = "ITFL",
            AirportIcao = "VHHH",
            RegionCode = "VH",
            RunwayIdentifier = "25L",
            Name = "ILS-cat-II",
            EncodedCourse = 91690.804,
            RawLine = "4 22.295272222 113.895083333 20 11090 18 91690.804 ITFL VHHH VH 25L ILS-cat-II"
        };

        var match = new LocalizerRunwayMatch(
            localizer,
            null!,
            runway);

        var corrector = new LocalizerCorrector();

        var result = corrector.Correct(match);

        Assert.True(result.IsCorrected, result.SkipReason);

        Assert.NotNull(result.CorrectedLatitude);
        Assert.NotNull(result.CorrectedLongitude);
        Assert.NotNull(result.CorrectedEncodedCourse);
        Assert.NotNull(result.LateralDisplacementMeters);


        // Expected WGS84 correction for VHHH RWY 25L.
        Assert.InRange(
            Math.Abs(result.CorrectedLatitude!.Value - 22.295257128),
            0.0,
            1e-7);

        Assert.InRange(
            Math.Abs(result.CorrectedLongitude!.Value - 113.895088957),
            0.0,
            1e-7);

        Assert.InRange(
            Math.Abs(result.CorrectedEncodedCourse!.Value - 91690.893),
            0.0,
            0.001);


        // The original localizer must not be modified.
        Assert.Equal(
            new GeoCoordinate(22.295272222, 113.895083333),
            localizer.Position);

        Assert.Equal(91690.804, localizer.EncodedCourse);
        Assert.Equal(20, localizer.ElevationFeet);
    }


    [Fact]
    public void Correct_RunwayWithIdenticalEndpoints_SkipsCorrection()
    {
        var runway = new Runway
        {
            Identifier = "36",
            StartPosition = new GeoCoordinate(0, 0),
            EndPosition = new GeoCoordinate(0, 0)
        };

        var localizer = new Localizer
        {
            RowCode = 4,
            Position = new GeoCoordinate(-0.001, 0),
            ElevationFeet = 100,
            Frequency = 11050,
            RangeNm = 18,
            Identifier = "ITST",
            AirportIcao = "TEST",
            RegionCode = "XX",
            RunwayIdentifier = "36",
            Name = "ILS-cat-I",
            EncodedCourse = 36000,
            RawLine = "Synthetic test localizer"
        };

        var match = new LocalizerRunwayMatch(localizer, null!, runway);

        var corrector = new LocalizerCorrector();

        var result = corrector.Correct(match);

        Assert.False(result.IsCorrected);
        Assert.NotNull(result.SkipReason);
        Assert.Contains(
            "Invalid runway geometry",
            result.SkipReason);

        Assert.Null(result.CorrectedLatitude);
        Assert.Null(result.CorrectedLongitude);
        Assert.Null(result.CorrectedEncodedCourse);
    }

    [Fact]
    public void ProjectOntoRunway_NorthboundRunway_PreservesMeridian()
    {
        var start = new GeoCoordinate(0, 0);
        var end = new GeoCoordinate(0.01, 0);

        var runwayPath = Wgs84Geodesic.Inverse(start, end);

        Assert.NotNull(runwayPath);

        var localizer = new GeoCoordinate(-0.00090437, 0);

        var result = Wgs84Geodesic.ProjectOntoRunway(
            start,
            runwayPath.Value.Bearing,
            runwayPath.Value.Distance,
            localizer);

        Assert.NotNull(result);

        Assert.InRange(
            Math.Abs(result.Value.Position.Longitude),
            0,
            1e-8);

        Assert.InRange(
            result.Value.Distance,
            0,
            0.01);
    }

    [Fact]
    public void Correct_LocalizerAlreadyOnCenterline_PreservesPosition()
    {
        var runway = new Runway
        {
            Identifier = "36",
            StartPosition = new GeoCoordinate(0, 0),
            EndPosition = new GeoCoordinate(0.01, 0)
        };

        // Place the localizer 100 metres south of the threshold
        // along the WGS84 runway centerline.
        var position = Wgs84Geodesic.Direct(
            runway.StartPosition,
            180.0,
            100.0);

        Assert.NotNull(position);

        var localizer = new Localizer
        {
            RowCode = 4,
            Position = position.Value,
            ElevationFeet = 100,
            Frequency = 11050,
            RangeNm = 18,
            Identifier = "ITST",
            AirportIcao = "TEST",
            RegionCode = "XX",
            RunwayIdentifier = "36",
            Name = "ILS-cat-I",
            EncodedCourse = 36000,
            RawLine = "Synthetic centered localizer"
        };

        var match = new LocalizerRunwayMatch(localizer, null!, runway);
        var corrector = new LocalizerCorrector();

        var result = corrector.Correct(match);

        Assert.True(result.IsCorrected, result.SkipReason);
        Assert.NotNull(result.CorrectedLatitude);
        Assert.NotNull(result.CorrectedLongitude);
        Assert.NotNull(result.LateralDisplacementMeters);
        Assert.NotNull(result.CorrectedEncodedCourse);

        // Allow 1 mm for numerical optimization error.
        Assert.InRange(
            result.LateralDisplacementMeters.Value,
            0.0,
            0.001);

        var corrected = new GeoCoordinate(
            result.CorrectedLatitude.Value,
            result.CorrectedLongitude.Value);

        var difference = Wgs84Geodesic.Inverse(
            localizer.Position,
            corrected);

        Assert.NotNull(difference);
        Assert.InRange(difference.Value.Distance, 0.0, 0.001);

        Assert.Equal(36000, result.CorrectedEncodedCourse.Value, 6);

        // The input object remains unchanged.
        Assert.Equal(position.Value, localizer.Position);
    }

    [Theory]
    [InlineData(double.NaN, 0)]
    [InlineData(double.PositiveInfinity, 0)]
    [InlineData(91, 0)]
    [InlineData(0, 181)]
    public void Correct_InvalidLocalizerCoordinates_SkipsCorrection(
        double latitude,
        double longitude)
    {
        var runway = new Runway
        {
            Identifier = "36",
            StartPosition = new GeoCoordinate(0, 0),
            EndPosition = new GeoCoordinate(0.01, 0)
        };

        var localizer = new Localizer
        {
            RowCode = 4,
            Position = new GeoCoordinate(latitude, longitude),
            ElevationFeet = 100,
            Frequency = 11050,
            RangeNm = 18,
            Identifier = "ITST",
            AirportIcao = "TEST",
            RegionCode = "XX",
            RunwayIdentifier = "36",
            Name = "ILS-cat-I",
            EncodedCourse = 36000,
            RawLine = "Synthetic invalid localizer"
        };

        var match = new LocalizerRunwayMatch(localizer, null!, runway);
        var corrector = new LocalizerCorrector();

        var result = corrector.Correct(match);

        Assert.False(result.IsCorrected);
        Assert.Contains(
            "Invalid localizer coordinates",
            result.SkipReason);

        Assert.Null(result.CorrectedLatitude);
        Assert.Null(result.CorrectedLongitude);
        Assert.Null(result.CorrectedEncodedCourse);
    }

    [Fact]
    public void Correct_LocalizerFarFromRunway_SkipsCorrection()
    {
        var runway = new Runway
        {
            Identifier = "36",
            StartPosition = new GeoCoordinate(0, 0),
            EndPosition = new GeoCoordinate(0.01, 0)
        };

        var localizer = new Localizer
        {
            RowCode = 4,
            Position = new GeoCoordinate(0, 180),
            ElevationFeet = 100,
            Frequency = 11050,
            RangeNm = 18,
            Identifier = "ITST",
            AirportIcao = "TEST",
            RegionCode = "XX",
            RunwayIdentifier = "36",
            Name = "ILS-cat-I",
            EncodedCourse = 36000,
            RawLine = "Synthetic out-of-hemisphere localizer"
        };

        var match = new LocalizerRunwayMatch(localizer, null!, runway);
        var corrector = new LocalizerCorrector();

        var result = corrector.Correct(match);

        Assert.False(result.IsCorrected);

        Assert.NotNull(result.SkipReason);

        Assert.Equal(
            "Lateral displacement exceeds 50 metres.",
            result.SkipReason);

        Assert.NotNull(result.LateralDisplacementMeters);

        Assert.True(
            result.LateralDisplacementMeters.Value > 50.0);

        Assert.Null(result.CorrectedLatitude);
        Assert.Null(result.CorrectedLongitude);
        Assert.Null(result.CorrectedEncodedCourse);
    }
}