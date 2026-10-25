using XPlaneLocalizerFixer.Geometry;

namespace XPlaneLocalizerFixer.Tests.Geometry;

public class Vector2DTests
{
    [Fact]
    public void Addition_ReturnsSum()
    {
        var a = new Vector2D(10, 20);
        var b = new Vector2D(5, 15);

        Assert.Equal(new Vector2D(15, 35), a + b);
    }

    [Fact]
    public void Subtraction_ReturnsDifference()
    {
        var a = new Vector2D(10, 20);
        var b = new Vector2D(5, 15);

        Assert.Equal(new Vector2D(5, 5), a - b);
    }

    [Fact]
    public void Multiplication_ScalesVector()
    {
        var vector = new Vector2D(3, 4);

        Assert.Equal(new Vector2D(6, 8), vector * 2);
    }

    [Fact]
    public void Division_ScalesVector()
    {
        var vector = new Vector2D(6, 8);

        Assert.Equal(new Vector2D(3, 4), vector / 2);
    }

    [Fact]
    public void Dot_ReturnsDotProduct()
    {
        var a = new Vector2D(3, 4);
        var b = new Vector2D(5, 6);

        Assert.Equal(39, a.Dot(b));
    }

    [Fact]
    public void Length_ReturnsMagnitude()
    {
        var vector = new Vector2D(3, 4);

        Assert.Equal(5, vector.Length);
    }

    [Fact]
    public void DistanceTo_ReturnsEuclideanDistance()
    {
        var a = new Vector2D(1, 2);
        var b = new Vector2D(4, 6);

        Assert.Equal(5, a.DistanceTo(b));
    }
}