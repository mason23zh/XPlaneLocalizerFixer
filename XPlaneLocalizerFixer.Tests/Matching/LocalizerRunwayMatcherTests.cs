using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Services;

namespace XPlaneLocalizerFixer.Tests.Matching;

public class LocalizerRunwayMatcherTests
{
    [Fact]
    public void Match_WhenAirportDoesNotExist_ReturnsNull()
    {
        // Arrange
        var localizer = CreateLocalizer();
        var airports = Array.Empty<Airport>();
        var matcher = new LocalizerRunwayMatcher();

        // Act
        var result = matcher.Match(localizer, airports);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Match_WhenAirportAndRunwayExist_ReturnsCorrectMatch()
    {
        // Arrange
        var localizer = CreateLocalizer();

        var runway = new Runway
        {
            Identifier = "25C",
            StartPosition = default,
            EndPosition = default
        };

        var airport = new Airport
        {
            IcaoCode = "VHHH",
            Runways =
            [
                runway
            ],
            Identifier = "VHHH",
            Name = "Hong Kong Intl"
        };

        var matcher = new LocalizerRunwayMatcher();

        // Act
        var result = matcher.Match(localizer, [airport]);

        // Assert
        Assert.NotNull(result);
        Assert.Same(localizer, result.Localizer);
        Assert.Same(airport, result.Airport);
        Assert.Same(runway, result.Runway);
    }

    [Fact]
    public void Match_WhenRunwayDoesNotExist_ReturnsNull()
    {
        // Arrange
        var localizer = CreateLocalizer();

        var runway = new Runway
        {
            Identifier = "25L",
            StartPosition = default,
            EndPosition = default
        };

        var airport = new Airport
        {
            IcaoCode = "VHHH",
            Identifier = "VHHH",
            Name = "Hong Kong Intl",
            Runways = [runway]
        };

        var matcher = new LocalizerRunwayMatcher();

        // Act
        var result = matcher.Match(localizer, [airport]);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("VHHH")]
    [InlineData("vhhh")]
    [InlineData("Vhhh")]
    [InlineData("vhHH")]
    public void Match_WhenAirportIcaoHasDifferentCasing_ReturnsMatch(
        string airportIcao)
    {
        // Arrange
        var localizer = CreateLocalizer();

        var runway = new Runway
        {
            Identifier = "25C",
            StartPosition = default,
            EndPosition = default
        };

        var airport = new Airport
        {
            IcaoCode = airportIcao,
            Identifier = airportIcao,
            Name = "Hong Kong Intl",
            Runways = [runway]
        };

        var matcher = new LocalizerRunwayMatcher();

        // Act
        var result = matcher.Match(localizer, [airport]);

        // Assert 
        Assert.NotNull(result);
        Assert.Same(airport, result.Airport);
        Assert.Same(runway, result.Runway);
    }

    [Fact]
    public void Match_WhenAirportIcaoIsNull_UsesAirportIdentifier()
    {
        // Arrange
        var localizer = CreateLocalizer();

        var runway = new Runway
        {
            Identifier = "25C",
            StartPosition = default,
            EndPosition = default
        };

        var airport = new Airport
        {
            IcaoCode = null,
            Identifier = "VHHH",
            Name = "Hong Kong Intl",
            Runways = [runway]
        };

        var matcher = new LocalizerRunwayMatcher();

        // Act
        var result = matcher.Match(localizer, [airport]);

        // Assert
        Assert.NotNull(result);
        Assert.Same(airport, result.Airport);
        Assert.Same(runway, result.Runway);
    }

    [Theory]
    [InlineData("VHHH", null)]
    [InlineData("VHHH", "")]
    [InlineData("VHHH", "   ")]
    public void Match_WhenLocalizerIdentifierIsMissing_ReturnsNull(
        string? airportIcao,
        string? runwayIdentifier)
    {
        // Arrange
        var localizer = CreateLocalizer(
            airportIcao,
            runwayIdentifier);

        var matcher = new LocalizerRunwayMatcher();

        // Act
        var result = matcher.Match(localizer, []);

        // Assert
        Assert.Null(result);
    }

    private static Localizer CreateLocalizer(
        string? airportIcao = "VHHH",
        string? runwayIdentifier = "25C")
    {
        return new Localizer
        {
            AirportIcao = airportIcao,
            RunwayIdentifier = runwayIdentifier!,
            Name = "Hong Kong Intl",
            EncodedCourse = 0,
            RowCode = 4,
            Position = default,
            ElevationFeet = 0,
            Frequency = 0,
            RangeNm = 0,
            Identifier = "ITFC",
            RegionCode = "VH",
            RawLine = "4  22.308577778  113.890702778       21    10930    18  91690.800 ITFC VHHH VH 25C ILS-cat-II"
        };
    }

    [Theory]
    [InlineData("7", "07")]
    [InlineData("8", "08")]
    [InlineData("1L", "01L")]
    [InlineData("2R", "02R")]
    [InlineData("02L", "02L")]
    [InlineData("16R", "16R")]
    public void Match_WhenRunwayIdentifiersHaveDifferentLeadingZeros_ReturnsMatch(
        string airportRunwayIdentifier,
        string localizerRunwayIdentifier)
    {
        // Arrange
        var localizer = new Localizer
        {
            AirportIcao = "KDEN",
            RunwayIdentifier = localizerRunwayIdentifier,
            Name = "DENVER INTL",
            EncodedCourse = 0,
            RowCode = 0,
            Position = default,
            ElevationFeet = 0,
            Frequency = 0,
            RangeNm = 0,
            Identifier = "IDXU",
            RegionCode = "K2",
            RawLine = " 4  39.840633333 -104.680294444     5341    11155    18  29610.513 IDZG KDEN K2 07 ILS-cat-I"
        };

        var runway = new Runway
        {
            Identifier = airportRunwayIdentifier,
            StartPosition = default,
            EndPosition = default
        };

        var airport = new Airport
        {
            Identifier = "KDEN",
            Runways =
            [
                runway
            ],
            Name = "DENVER INTL"
        };


        var matcher = new LocalizerRunwayMatcher();

        // Act
        var result = matcher.Match(localizer, [airport]);

        // Assert
        Assert.NotNull(result);
        Assert.Same(localizer, result.Localizer);
        Assert.Same(airport, result.Airport);
        Assert.Same(runway, result.Runway);
    }
}