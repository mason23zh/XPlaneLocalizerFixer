using XPlaneLocalizerFixer.Parsers;

namespace XPlaneLocalizerFixer.Tests.Parsers;

public class EarthNavDataParserTests
{
    [Fact]
    public void Parse_WhenLocalizerRecordExists_ParsesLocalizerCorrectly()
    {
        // Arrange
        string[] lines =
        [
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I"
        ];

        var parser = new EarthNavDatParser();

        // Act
        var navData = parser.Parse(lines);

        // Assert
        var localizer = Assert.Single(navData.Localizers);

        Assert.Equal(4, localizer.RowCode);
        Assert.Equal(-22.830111111, localizer.Position.Latitude);
        Assert.Equal(-43.236972222, localizer.Position.Longitude);
        Assert.Equal(17, localizer.ElevationFeet);
        Assert.Equal(11050, localizer.Frequency);
        Assert.Equal(18, localizer.RangeNm);
        Assert.Equal(53765.679, localizer.EncodedCourse);
        Assert.Equal("IJB", localizer.Identifier);
        Assert.Equal("SBGL", localizer.AirportIcao);
        Assert.Equal("SB", localizer.RegionCode);
        Assert.Equal("15", localizer.RunwayIdentifier);
        Assert.Equal("ILS-cat-I", localizer.Name);
    }

    [Fact]
    public void Parse_WhenMultipleLocalizersExist_ParsesAllLocalizers()
    {
        // Arrange
        string[] lines =
        [
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I",
            "4 22.308577778 113.890702778 21 10930 18 91690.800 ITFC VHHH VH 25C ILS-cat-II"
        ];

        var parser = new EarthNavDatParser();

        // Act
        var navData = parser.Parse(lines);

        // Assert
        Assert.Equal(2, navData.Localizers.Count);

        var sbgl = Assert.Single(
            navData.Localizers,
            loc => loc.AirportIcao == "SBGL");

        var vhhh = Assert.Single(
            navData.Localizers,
            loc => loc.AirportIcao == "VHHH");

        Assert.Equal("15", sbgl.RunwayIdentifier);
        Assert.Equal("IJB", sbgl.Identifier);
        Assert.Equal(-22.830111111, sbgl.Position.Latitude);
        Assert.Equal(-43.236972222, sbgl.Position.Longitude);

        Assert.Equal("25C", vhhh.RunwayIdentifier);
        Assert.Equal("ITFC", vhhh.Identifier);
        Assert.Equal(22.308577778, vhhh.Position.Latitude);
        Assert.Equal(113.890702778, vhhh.Position.Longitude);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    public void Parse_WhenLocalizerHasDifferentRowCode_ParsesCorrectly(
        int rowCode)
    {
        // Arrange
        string[] lines =
        [
            $"{rowCode} -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I"
        ];

        var parser = new EarthNavDatParser();

        // Act
        var navData = parser.Parse(lines);

        // Assert
        var localizer = Assert.Single(navData.Localizers);

        Assert.Equal(rowCode, localizer.RowCode);
        Assert.Equal("SBGL", localizer.AirportIcao);
        Assert.Equal("15", localizer.RunwayIdentifier);
        Assert.Equal(53765.679, localizer.EncodedCourse);
    }

    [Fact]
    public void Parse_WhenGlideslopeRecordExists_ParsesGlideslopeCorrectly()
    {
        // Arrange
        string[] lines =
        [
            "6 -22.815583333 -43.261000000 17 11050 18 300125.679 IJB SBGL SB 15 GS"
        ];

        var parser = new EarthNavDatParser();

        // Act
        var navData = parser.Parse(lines);

        // Assert
        var glideslope = Assert.Single(navData.GlideSlopes);

        Assert.Equal(-22.815583333, glideslope.Position.Latitude);
        Assert.Equal(-43.261000000, glideslope.Position.Longitude);
        Assert.Equal(17, glideslope.ElevationFeet);
        Assert.Equal(11050, glideslope.Frequency);
        Assert.Equal(300125.679, glideslope.EncodingBearing);
        Assert.Equal("IJB", glideslope.Identifier);
        Assert.Equal("SBGL", glideslope.AirportIcao);
        Assert.Equal("15", glideslope.RunwayIdentifier);
    }

    [Fact]
    public void Parse_WhenDmeRecordExists_ParsesDmeCorrectly()
    {
        // Arrange
        string[] lines =
        [
            "12 -22.829694444 -43.236472222 23 11050 25 0.000 IJB SBGL SB GALEAO-ANTONIO C JOBIM IN DME-ILS"
        ];

        var parser = new EarthNavDatParser();

        // Act
        var navData = parser.Parse(lines);

        // Assert
        var dme = Assert.Single(navData.Dmes);

        Assert.Equal(-22.829694444, dme.Position.Latitude);
        Assert.Equal(-43.236472222, dme.Position.Longitude);
        Assert.Equal(23, dme.ElevationFeet);
        Assert.Equal(11050, dme.Frequency);
        Assert.Equal(25, dme.RangeNm);
        Assert.Equal("IJB", dme.Identifier);
        Assert.Equal("SBGL", dme.AirportIcao);
        Assert.Equal("SB", dme.RegionCode);
    }

    [Fact]
    public void Parse_WhenIlsRecordsExist_ParsesAllNavigationTypes()
    {
        // Arrange
        string[] lines =
        [
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I",
            "6 -22.815583333 -43.261000000 17 11050 18 300125.679 IJB SBGL SB 15 GS",
            "12 -22.829694444 -43.236472222 23 11050 25 0.000 IJB SBGL SB GALEAO-ANTONIO C JOBIM IN DME-ILS"
        ];

        var parser = new EarthNavDatParser();

        // Act
        var navData = parser.Parse(lines);

        // Assert
        var localizer = Assert.Single(navData.Localizers);
        var glideslope = Assert.Single(navData.GlideSlopes);
        var dme = Assert.Single(navData.Dmes);

        Assert.Equal(4, localizer.RowCode);
        Assert.Equal(6, glideslope.RowCode);
        Assert.Equal(12, dme.RowCode);

        Assert.Equal("IJB", localizer.Identifier);
        Assert.Equal("IJB", glideslope.Identifier);
        Assert.Equal("IJB", dme.Identifier);

        Assert.Equal(11050, localizer.Frequency);
        Assert.Equal(11050, glideslope.Frequency);
        Assert.Equal(11050, dme.Frequency);
    }

    [Fact]
    public void Parse_WhenNavigationRecordsExist_PreservesOriginalRawLines()
    {
        // Arrange
        const string localizerLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        const string glideslopeLine =
            "6 -22.815583333 -43.261000000 17 11050 18 300125.679 IJB SBGL SB 15 GS";

        const string dmeLine =
            "12 -22.829694444 -43.236472222 23 11050 25 0.000 IJB SBGL SB GALEAO-ANTONIO C JOBIM IN DME-ILS";

        var parser = new EarthNavDatParser();

        // Act
        var navData = parser.Parse(
            [localizerLine, glideslopeLine, dmeLine]);

        // Assert
        var localizer = Assert.Single(navData.Localizers);
        var glideslope = Assert.Single(navData.GlideSlopes);
        var dme = Assert.Single(navData.Dmes);

        Assert.Equal(localizerLine, localizer.RawLine);
        Assert.Equal(glideslopeLine, glideslope.RawLine);
        Assert.Equal(dmeLine, dme.RawLine);
    }

    [Fact]
    public void Parse_WhenLocalizerRecordIsIncomplete_ThrowsFormatException()
    {
        // Arrange
        string[] lines =
        [
            "4 -22.830111111 -43.236972222 17 11050"
        ];

        var parser = new EarthNavDatParser();

        // Act
        var exception = Assert.Throws<FormatException>(() => parser.Parse(lines));

        // Assert
        Assert.NotEmpty(exception.Message);
    }
}