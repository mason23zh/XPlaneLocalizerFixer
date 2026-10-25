namespace XPlaneLocalizerFixer.Geometry;

/// <summary>
///     Provides geographic bearing and heading calculations.
///     All angles are expressed in degrees.
/// </summary>
/// <remarks>
///     For further reference,
///     check https://codeberg.org/skiselkov/libacfutils/src/commit/0c08a27154491027feda2bc76b47ae9b317c10c0/src/geom.c
/// </remarks>
public static class GeoMath
{
    private const double DegreesToRadians = Math.PI / 180.0;
    private const double RadiansToDegrees = 180.0 / Math.PI;

    /// <summary>
    ///     Calculates the initial true bearing between two geographic positions.
    /// </summary>
    /// <param name="startLatitude">Starting latitude in degrees.</param>
    /// <param name="startLongitude">Starting longitude in degrees.</param>
    /// <param name="endLatitude">Destination latitude in degrees.</param>
    /// <param name="endLongitude">Destination longitude in degrees.</param>
    /// <returns>The initial true bearing, normalized to [0, 360) degrees.</returns>
    /// <remarks>gc_point_hdg() in geom.c</remarks>
    public static double CalculateBearing(
        double startLatitude,
        double startLongitude,
        double endLatitude,
        double endLongitude)
    {
        ValidateCoordinates(startLatitude, startLongitude);
        ValidateCoordinates(endLatitude, endLongitude);

        if (startLatitude == endLatitude &&
            startLongitude == endLongitude)
            throw new ArgumentException(
                "Cannot calculate a bearing between identical positions.");

        var lat1 = startLatitude * DegreesToRadians;
        var lat2 = endLatitude * DegreesToRadians;
        var deltaLon = (endLongitude - startLongitude) * DegreesToRadians;

        var y = Math.Sin(deltaLon) * Math.Cos(lat2);

        var x =
            Math.Cos(lat1) * Math.Sin(lat2) -
            Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(deltaLon);

        var bearing = Math.Atan2(y, x) * RadiansToDegrees;

        return NormalizeHeading(bearing);
    }

    /// <summary>
    ///     Normalizes a heading to the range [0, 360) degrees.
    /// </summary>
    /// <param name="heading">Heading in degrees.</param>
    /// <returns>The normalized heading.</returns>
    /// <remarks>normalize_hdg() in geom.c</remarks>
    public static double NormalizeHeading(double heading)
    {
        if (!double.IsFinite(heading))
            throw new ArgumentOutOfRangeException(
                nameof(heading),
                "Heading must be a finite number.");

        var normalized = heading % 360.0;

        if (normalized < 0)
            normalized += 360.0;

        return normalized;
    }

    /// <summary>
    ///     Calculates the smallest unsigned angular difference between two headings.
    /// </summary>
    /// <param name="first">First heading in degrees.</param>
    /// <param name="second">Second heading in degrees.</param>
    /// <returns>The angular difference between 0 and 180 degrees.</returns>
    /// <remarks>rel_hdg() in geom.c</remarks>
    public static double AngularDifference(double first, double second)
    {
        var difference = Math.Abs(
            NormalizeHeading(first) - NormalizeHeading(second));

        return Math.Min(difference, 360.0 - difference);
    }

    private static void ValidateCoordinates(double latitude, double longitude)
    {
        if (!double.IsFinite(latitude) || latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude));

        if (!double.IsFinite(longitude) || longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude));
    }
}