using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Services;

namespace XPlaneLocalizerFixer.Tests.Services;

public class LocalizerNavAdMatcherTest
{
    [Fact]
    public void Match_WhenLocalizerHasGlideSlopeAndDme_ReturnsBoth()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        const string gsLine =
            "6 -22.815583333 -43.261000000 17 11050 18 300125.679 IJB SBGL SB 15 GS";

        const string dmeLine =
            "12 -22.829694444 -43.236472222 23 11050 25 0.000 IJB SBGL SB GALEAO-ANTONIO C JOBIM IN DME-ILS";

        var parser = new EarthNavDatParser();

        var navData = parser.Parse(
        [
            locLine,
            gsLine,
            dmeLine
        ]);

        var localizer = Assert.Single(navData.Localizers);
        var originalGlideSlope = Assert.Single(navData.GlideSlopes);
        var originalDme = Assert.Single(navData.Dmes);

        var matcher = new LocalizerNavAidMatcher();

        // Act
        var result = matcher.Match(localizer, navData);

        // Assert
        Assert.Same(localizer, result.Localizer);
        Assert.Same(originalGlideSlope, result.GlideSlope);
        Assert.Same(originalDme, result.Dme);

        Assert.True(result.IsEligibleForCorrection);

        // Matching must preserve the original navigation records.
        Assert.Equal(gsLine, result.GlideSlope.RawLine);
        Assert.Equal(dmeLine, result.Dme.RawLine);
    }

    [Fact]
    public void Match_WhenGlideSlopeIsMissing_ReturnsIneligibleMatch()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        const string dmeLine =
            "12 -22.829694444 -43.236472222 23 11050 25 0.000 IJB SBGL SB GALEAO-ANTONIO C JOBIM IN DME-ILS";

        var parser = new EarthNavDatParser();

        var navData = parser.Parse(
        [
            locLine,
            dmeLine
        ]);

        var localizer = Assert.Single(navData.Localizers);
        var matcher = new LocalizerNavAidMatcher();

        // Act
        var result = matcher.Match(localizer, navData);

        // Assert
        Assert.Same(localizer, result.Localizer);
        Assert.Null(result.GlideSlope);
        Assert.Null(result.Dme);
        Assert.False(result.IsEligibleForCorrection);
    }

    // Row 5 means stand-alone loc
    [Fact]
    public void Match_WhenLocalizerIsRow5_ReturnsIneligibleMatch()
    {
        // Arrange
        const string locLine =
            "5 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 LOC";

        const string gsLine =
            "6 -22.815583333 -43.261000000 17 11050 18 300125.679 IJB SBGL SB 15 GS";

        const string dmeLine =
            "12 -22.829694444 -43.236472222 23 11050 25 0.000 IJB SBGL SB GALEAO-ANTONIO C JOBIM IN DME-ILS";

        var parser = new EarthNavDatParser();

        var navData = parser.Parse(
        [
            locLine,
            gsLine,
            dmeLine
        ]);

        var localizer = Assert.Single(navData.Localizers);
        var matcher = new LocalizerNavAidMatcher();

        // Act
        var result = matcher.Match(localizer, navData);

        // Assert
        Assert.Same(localizer, result.Localizer);
        Assert.Null(result.GlideSlope);
        Assert.Null(result.Dme);
        Assert.False(result.IsEligibleForCorrection);
    }

    [Fact]
    public void Match_WhenDmeIsMissing_ReturnsEligibleMatch()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        const string gsLine =
            "6 -22.815583333 -43.261000000 17 11050 18 300125.679 IJB SBGL SB 15 GS";

        var parser = new EarthNavDatParser();

        var navData = parser.Parse(
        [
            locLine,
            gsLine
        ]);

        var localizer = Assert.Single(navData.Localizers);
        var originalGlideSlope = Assert.Single(navData.GlideSlopes);

        var matcher = new LocalizerNavAidMatcher();

        // Act
        var result = matcher.Match(localizer, navData);

        // Assert
        Assert.Same(localizer, result.Localizer);
        Assert.Same(originalGlideSlope, result.GlideSlope);
        Assert.Null(result.Dme);

        Assert.True(result.IsEligibleForCorrection);

        // The original glideslope record must remain unchanged.
        Assert.Equal(gsLine, result.GlideSlope!.RawLine);
    }

    [Fact]
    public void Match_WhenGlideSlopeIdentifierDiffers_ReturnsIneligibleMatch()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        // Same airport, runway and frequency, but a different identifier.
        const string gsLine =
            "6 -22.815583333 -43.261000000 17 11050 18 300125.679 XYZ SBGL SB 15 GS";

        var parser = new EarthNavDatParser();

        var navData = parser.Parse(
        [
            locLine,
            gsLine
        ]);

        var localizer = Assert.Single(navData.Localizers);
        var matcher = new LocalizerNavAidMatcher();

        // Act
        var result = matcher.Match(localizer, navData);

        // Assert
        Assert.Same(localizer, result.Localizer);
        Assert.Null(result.GlideSlope);
        Assert.Null(result.Dme);
        Assert.False(result.IsEligibleForCorrection);
    }

    [Fact]
    public void Match_WhenGlideSlopeFrequencyDiffers_ReturnsIneligibleMatch()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        // Same identifier, airport and runway, but different frequency. ILS paired Loc should share the same freq.
        const string gsLine =
            "6 -22.815583333 -43.261000000 17 11150 18 300125.679 IJB SBGL SB 15 GS";

        var parser = new EarthNavDatParser();

        var navData = parser.Parse(
        [
            locLine,
            gsLine
        ]);

        var localizer = Assert.Single(navData.Localizers);
        var matcher = new LocalizerNavAidMatcher();

        // Act
        var result = matcher.Match(localizer, navData);

        // Assert
        Assert.Same(localizer, result.Localizer);
        Assert.Null(result.GlideSlope);
        Assert.Null(result.Dme);
        Assert.False(result.IsEligibleForCorrection);
    }

    [Fact]
    public void Match_WhenGlideSlopeRunwayDiffers_ReturnsIneligibleMatch()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        // Same identifier, airport and frequency, but a different runway.
        const string gsLine =
            "6 -22.815583333 -43.261000000 17 11050 18 300125.679 IJB SBGL SB 28 GS";

        var parser = new EarthNavDatParser();

        var navData = parser.Parse(
        [
            locLine,
            gsLine
        ]);

        var localizer = Assert.Single(navData.Localizers);
        var matcher = new LocalizerNavAidMatcher();

        // Act
        var result = matcher.Match(localizer, navData);

        // Assert
        Assert.Same(localizer, result.Localizer);
        Assert.Null(result.GlideSlope);
        Assert.Null(result.Dme);
        Assert.False(result.IsEligibleForCorrection);
    }

    [Fact]
    public void Match_WhenGlideSlopeAirportDiffers_ReturnsIneligibleMatch()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        // Same identifier, runway, frequency and region, but different airport.
        const string gsLine =
            "6 -22.815583333 -43.261000000 17 11050 18 300125.679 IJB SBRJ SB 15 GS";

        var parser = new EarthNavDatParser();

        var navData = parser.Parse(
        [
            locLine,
            gsLine
        ]);

        var localizer = Assert.Single(navData.Localizers);
        var matcher = new LocalizerNavAidMatcher();

        // Act
        var result = matcher.Match(localizer, navData);

        // Assert
        Assert.Same(localizer, result.Localizer);
        Assert.Null(result.GlideSlope);
        Assert.Null(result.Dme);
        Assert.False(result.IsEligibleForCorrection);
    }

    [Fact]
    public void Match_WhenGlideSlopeRegionDiffers_ReturnsIneligibleMatch()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        // Same airport, identifier, runway and frequency, but different region.
        const string gsLine =
            "6 -22.815583333 -43.261000000 17 11050 18 300125.679 IJB SBGL VH 15 GS";

        var parser = new EarthNavDatParser();

        var navData = parser.Parse(
        [
            locLine,
            gsLine
        ]);

        var localizer = Assert.Single(navData.Localizers);
        var matcher = new LocalizerNavAidMatcher();

        // Act
        var result = matcher.Match(localizer, navData);

        // Assert
        Assert.Same(localizer, result.Localizer);
        Assert.Null(result.GlideSlope);
        Assert.Null(result.Dme);
        Assert.False(result.IsEligibleForCorrection);
    }

    [Fact]
    public void Match_WhenMultipleGlideSlopesMatch_ThrowsInvalidOperationException()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        const string gsLine1 =
            "6 -22.815583333 -43.261000000 17 11050 18 300125.679 IJB SBGL SB 15 GS";

        // Same identifying fields, but different coordinates.
        const string gsLine2 =
            "6 -22.815600000 -43.261100000 17 11050 18 300125.679 IJB SBGL SB 15 GS";

        var parser = new EarthNavDatParser();

        var navData = parser.Parse(
        [
            locLine,
            gsLine1,
            gsLine2
        ]);

        var localizer = Assert.Single(navData.Localizers);
        var matcher = new LocalizerNavAidMatcher();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => matcher.Match(localizer, navData));
    }

    [Fact]
    public void Match_WhenMultipleDmesMatch_ThrowsInvalidOperationException()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        const string gsLine =
            "6 -22.815583333 -43.261000000 17 11050 18 300125.679 IJB SBGL SB 15 GS";

        const string dmeLine1 =
            "12 -22.829694444 -43.236472222 23 11050 25 0.000 IJB SBGL SB GALEAO-ANTONIO C JOBIM IN DME-ILS";

        // Same identifying fields, but different coordinates.
        const string dmeLine2 =
            "12 -22.829700000 -43.236500000 23 11050 25 0.000 IJB SBGL SB GALEAO-ANTONIO C JOBIM IN DME-ILS";

        var parser = new EarthNavDatParser();

        var navData = parser.Parse(
        [
            locLine,
            gsLine,
            dmeLine1,
            dmeLine2
        ]);

        var localizer = Assert.Single(navData.Localizers);
        var matcher = new LocalizerNavAidMatcher();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => matcher.Match(localizer, navData));
    }

    [Fact]
    public void Match_WhenIdentifiersDifferOnlyByCase_ReturnsMatchingNavAids()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        // Identifiers, airport codes, regions and runway identifiers use lowercase.
        const string gsLine =
            "6 -22.815583333 -43.261000000 17 11050 18 300125.679 ijb sbgl sb 15 GS";

        const string dmeLine =
            "12 -22.829694444 -43.236472222 23 11050 25 0.000 ijb sbgl sb GALEAO-ANTONIO C JOBIM IN DME-ILS";

        var parser = new EarthNavDatParser();

        var navData = parser.Parse(
        [
            locLine,
            gsLine,
            dmeLine
        ]);

        var localizer = Assert.Single(navData.Localizers);
        var originalGlideSlope = Assert.Single(navData.GlideSlopes);
        var originalDme = Assert.Single(navData.Dmes);

        var matcher = new LocalizerNavAidMatcher();

        // Act
        var result = matcher.Match(localizer, navData);

        // Assert
        Assert.Same(localizer, result.Localizer);
        Assert.Same(originalGlideSlope, result.GlideSlope);
        Assert.Same(originalDme, result.Dme);

        Assert.True(result.IsEligibleForCorrection);

        // The original records must preserve their lowercase values.
        Assert.Equal(gsLine, result.GlideSlope!.RawLine);
        Assert.Equal(dmeLine, result.Dme!.RawLine);
    }

    [Fact]
    public void Match_WhenLocalizerIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var matcher = new LocalizerNavAidMatcher();
        var navData = new NavData();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => matcher.Match(null!, navData));

        Assert.Equal("localizer", exception.ParamName);
    }

    [Fact]
    public void Match_WhenNavDataIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        const string locLine =
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I";

        var parser = new EarthNavDatParser();
        var navData = parser.Parse([locLine]);

        var localizer = Assert.Single(navData.Localizers);
        var matcher = new LocalizerNavAidMatcher();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => matcher.Match(localizer, null!));

        Assert.Equal("navData", exception.ParamName);
    }
}