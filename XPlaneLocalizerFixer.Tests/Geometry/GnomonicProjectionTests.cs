using XPlaneLocalizerFixer.Geometry;

namespace XPlaneLocalizerFixer.Tests.Geometry;

public class GnomonicProjectionTests
{
    [Fact]
    public void Project_Center_ReturnsOrigin()
    {
        var projection = new GnomonicProjection(-22.8, -43.2);

        var result = projection.Project(-22.8, -43.2);

        Assert.Equal(0, result.X, 6);
        Assert.Equal(0, result.Y, 6);
    }

    [Fact]
    public void Project_PointEast_ReturnsPositiveX()
    {
        var projection = new GnomonicProjection(0, 0);

        var result = projection.Project(0, 0.001);

        Assert.True(result.X > 0);
        Assert.Equal(0, result.Y, 6);
    }

    [Fact]
    public void Project_PointNorth_ReturnsPositiveY()
    {
        var projection = new GnomonicProjection(0, 0);

        var result = projection.Project(0.001, 0);

        Assert.True(result.Y > 0);
        Assert.Equal(0, result.X, 6);
    }

    [Theory]
    [InlineData(-22.830111111, -43.236972222)]
    [InlineData(22.295272222, 113.895083333)]
    [InlineData(0, 0)]
    [InlineData(51.5, -0.12)]
    public void Project_Unproject_ReturnsOriginalCoordinates(
        double latitude,
        double longitude)
    {
        var projection = new GnomonicProjection(latitude + 0.01, longitude);

        var projected = projection.Project(latitude, longitude);
        var result = projection.Unproject(projected);

        Assert.Equal(latitude, result.Latitude, 8);
        Assert.Equal(longitude, result.Longitude, 8);
    }

    [Fact]
    public void Project_OppositeHemisphere_Throws()
    {
        var projection = new GnomonicProjection(0, 0);

        Assert.Throws<ArgumentOutOfRangeException>(() => projection.Project(0, 180));
    }

    [Fact]
    public void Unproject_Origin_ReturnsProjectionCenter()
    {
        var projection = new GnomonicProjection(-22.8, -43.2);

        var result = projection.Unproject(new Vector2D(0, 0));

        Assert.Equal(-22.8, result.Latitude, 8);
        Assert.Equal(-43.2, result.Longitude, 8);
    }
}