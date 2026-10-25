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
            Name = null
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
            Name = null,
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
            Name = null,
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
            Name = null,
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
        string airportIcao = "VHHH",
        string runwayIdentifier = "25C")
    {
        return new Localizer
        {
            AirportIcao = airportIcao,
            RunwayIdentifier = runwayIdentifier,
            Name = null,
            EncodedCourse = 0,
            RowCode = 4,
            Position = default,
            ElevationFeet = 0,
            Frequency = 0,
            RangeNm = 0,
            Identifier = null,
            RegionCode = null,
            RawLine = null
        };
    }
}