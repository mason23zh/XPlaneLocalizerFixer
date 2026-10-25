namespace XPlaneLocalizerFixer.Geometry;

/// <summary>
///     Performs forward and inverse spherical Gnomonic projections.
///     The projection center is the origin of the local coordinate system.
/// </summary>
/// <remarks>
///     For further detail: check geom.c in skiselkov/libacfutils
///     https://codeberg.org/skiselkov/libacfutils/src/commit/0c08a27154491027feda2bc76b47ae9b317c10c0/src/geom.c
/// </remarks>
public sealed class GnomonicProjection
{
    /*
     * 
    https://codeberg.org/skiselkov/libacfutils/src/commit/0c08a27154491027feda2bc76b47ae9b317c10c0/src/acfutils/geom.h#L284
     */
    private const double MeanEarthRadiusMeters = 6_371_200.0;
    private const double DegreesToRadians = Math.PI / 180.0;
    private const double RadiansToDegrees = 180.0 / Math.PI;

    private readonly double _centerLatitude;
    private readonly double _centerLongitude;
    private readonly double _cosCenterLatitude;
    private readonly double _sinCenterLatitude;

    /// <summary>
    ///     Initializes a Gnomonic projection centered on a geographic position.
    /// </summary>
    /// <param name="latitude">Projection center latitude in degrees.</param>
    /// <param name="longitude">Projection center longitude in degrees.</param>
    public GnomonicProjection(double latitude, double longitude)
    {
        ValidateCoordinates(latitude, longitude);

        _centerLatitude = latitude * DegreesToRadians;
        _centerLongitude = longitude * DegreesToRadians;

        _sinCenterLatitude = Math.Sin(_centerLatitude);
        _cosCenterLatitude = Math.Cos(_centerLatitude);
    }

    /// <summary>
    ///     Projects geographic coordinates onto the local Gnomonic plane.
    /// </summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <returns>Projected coordinates in metres, with X east and Y north.</returns>
    public Vector2D Project(double latitude, double longitude)
    {
        ValidateCoordinates(latitude, longitude);

        var lat = latitude * DegreesToRadians;
        var lon = longitude * DegreesToRadians;
        var deltaLon = lon - _centerLongitude;

        var sinLat = Math.Sin(lat);
        var cosLat = Math.Cos(lat);

        var cosC =
            _sinCenterLatitude * sinLat +
            _cosCenterLatitude * cosLat * Math.Cos(deltaLon);

        if (cosC <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(latitude),
                "The position is outside the valid Gnomonic projection hemisphere.");

        var x = MeanEarthRadiusMeters * cosLat * Math.Sin(deltaLon) / cosC;

        var y = MeanEarthRadiusMeters *
            (_cosCenterLatitude * sinLat -
             _sinCenterLatitude * cosLat * Math.Cos(deltaLon)) / cosC;

        return new Vector2D(x, y);
    }

    /// <summary>
    ///     Converts projected coordinates back into geographic coordinates.
    /// </summary>
    /// <param name="position">Projected position in metres.</param>
    /// <returns>Latitude and longitude in degrees.</returns>
    public (double Latitude, double Longitude) Unproject(Vector2D position)
    {
        if (!double.IsFinite(position.X) || !double.IsFinite(position.Y))
            throw new ArgumentOutOfRangeException(nameof(position));

        var rho = Math.Sqrt(
            position.X * position.X +
            position.Y * position.Y);

        if (rho == 0)
            return (
                _centerLatitude * RadiansToDegrees,
                _centerLongitude * RadiansToDegrees
            );

        var c = Math.Atan(rho / MeanEarthRadiusMeters);
        var sinC = Math.Sin(c);
        var cosC = Math.Cos(c);

        var latitude = Math.Asin(
            cosC * _sinCenterLatitude +
            position.Y * sinC * _cosCenterLatitude / rho);

        var longitude = _centerLongitude + Math.Atan2(
            position.X * sinC,
            rho * _cosCenterLatitude * cosC -
            position.Y * _sinCenterLatitude * sinC);

        var longitudeDegrees = longitude * RadiansToDegrees;
        longitudeDegrees = (longitudeDegrees + 180.0) % 360.0;

        if (longitudeDegrees < 0)
            longitudeDegrees += 360.0;

        longitudeDegrees -= 180.0;

        return (latitude * RadiansToDegrees, longitudeDegrees);
    }

    private static void ValidateCoordinates(double latitude, double longitude)
    {
        if (!double.IsFinite(latitude) || latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude));

        if (!double.IsFinite(longitude) || longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude));
    }
}