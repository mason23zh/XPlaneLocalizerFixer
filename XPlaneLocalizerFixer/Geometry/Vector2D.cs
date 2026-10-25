namespace XPlaneLocalizerFixer.Geometry;

/// <summary>
///     Represents an immutable 2D vector using double precision coordinates.
/// </summary>
/// <remarks>
///     Check geom.c in libacfutils for further reference
///     https://codeberg.org/skiselkov/libacfutils/src/commit/0c08a27154491027feda2bc76b47ae9b317c10c0/src/geom.c
/// </remarks>
/// <param name="X">The horizontal coordinate.</param>
/// <param name="Y">The vertical coordinate.</param>
public readonly record struct Vector2D(double X, double Y)
{
    /// <summary>
    ///     Gets the magnitude of the vector.
    /// </summary>
    public double Length => Math.Sqrt(X * X + Y * Y);

    /// <summary>
    ///     Calculates the dot product with another vector.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The scalar dot product.</returns>
    public double Dot(Vector2D other)
    {
        return X * other.X + Y * other.Y;
    }

    /// <summary>
    ///     Calculates the distance to another point.
    /// </summary>
    /// <param name="other">The other point.</param>
    /// <returns>The Euclidean distance.</returns>
    public double DistanceTo(Vector2D other)
    {
        return (this - other).Length;
    }

    public static Vector2D operator +(Vector2D left, Vector2D right)
    {
        return new Vector2D(left.X + right.X, left.Y + right.Y);
    }

    public static Vector2D operator -(Vector2D left, Vector2D right)
    {
        return new Vector2D(left.X - right.X, left.Y - right.Y);
    }

    public static Vector2D operator *(Vector2D vector, double scalar)
    {
        return new Vector2D(vector.X * scalar, vector.Y * scalar);
    }

    public static Vector2D operator /(Vector2D vector, double scalar)
    {
        return new Vector2D(vector.X / scalar, vector.Y / scalar);
    }
}