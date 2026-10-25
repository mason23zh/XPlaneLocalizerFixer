using XPlaneLocalizerFixer.Geometry;

namespace XPlaneLocalizerFixer.Tests.Geometry;

public class GeoMathTests
{
    [Theory]
    [InlineData(0, 0, 1, 0, 0)]
    [InlineData(0, 0, 0, 1, 90)]
    [InlineData(0, 0, -1, 0, 180)]
    [InlineData(0, 0, 0, -1, 270)]
    public void CalculateBearing_CardinalDirections_ReturnsExpectedBearing(
        double startLat,
        double startLon,
        double endLat,
        double endLon,
        double expected)
    {
        var result = GeoMath.CalculateBearing(
            startLat, startLon, endLat, endLon);

        Assert.Equal(expected, result, 6);
    }

    [Theory]
    [InlineData(360, 0)]
    [InlineData(450, 90)]
    [InlineData(-90, 270)]
    [InlineData(720, 0)]
    [InlineData(-450, 270)]
    public void NormalizeHeading_ReturnsExpectedHeading(
        double heading,
        double expected)
    {
        Assert.Equal(expected, GeoMath.NormalizeHeading(heading), 6);
    }

    [Theory]
    [InlineData(10, 20, 10)]
    [InlineData(350, 10, 20)]
    [InlineData(359.7, 0.2, 0.5)]
    [InlineData(90, 270, 180)]
    [InlineData(45, 45, 0)]
    public void AngularDifference_ReturnsSmallestAngle(
        double first,
        double second,
        double expected)
    {
        var result = GeoMath.AngularDifference(first, second);

        Assert.Equal(expected, result, 6);
    }

    [Fact]
    public void CalculateBearing_IdenticalCoordinates_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            GeoMath.CalculateBearing(22.3, 113.9, 22.3, 113.9));
    }

    [Fact]
    public void NormalizeHeading_NonFiniteValue_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GeoMath.NormalizeHeading(double.NaN));
    }
}