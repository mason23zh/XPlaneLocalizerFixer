using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Geometry;

/// <summary>
///     Provides WGS84 Vincenty geodesic calculations and perpendicular projection
///     onto an extended geodesic. All distances are expressed in metres.
/// </summary>
public static class Wgs84Geodesic
{
    private const double SemiMajorAxis = 6_378_137.0;
    private const double Flattening = 1.0 / 298.257223563;
    private const double SemiMinorAxis = SemiMajorAxis * (1.0 - Flattening);

    private const double DegreesToRadians = Math.PI / 180.0;
    private const double RadiansToDegrees = 180.0 / Math.PI;
    private const double ConvergenceTolerance = 1e-12;
    private const int MaxIterations = 200;

    private static readonly double GoldenRatio =
        (Math.Sqrt(5.0) - 1.0) / 2.0;

    /// <summary>
    ///     Finds the closest geographic position on the extended runway geodesic.
    /// </summary>
    public static (GeoCoordinate Position, double Distance)?
        ProjectOntoRunway(
            GeoCoordinate start,
            double runwayHeading,
            double runwayLength,
            GeoCoordinate nav)
    {
        // Include space beyond both runway ends for LOC transmitters.
        // This defines the search interval, not the runway segment.
        var lower = -25_000.0;
        var upper = runwayLength + 25_000.0;

        GeoCoordinate? PositionAt(double distance)
        {
            return Direct(start, runwayHeading, distance);
        }

        double DistanceSquared(double distance)
        {
            var position = PositionAt(distance);

            if (position is null)
                return double.PositiveInfinity;

            var inverse = Inverse(position.Value, nav);

            return inverse is null
                ? double.PositiveInfinity
                : inverse.Value.Distance * inverse.Value.Distance;
        }


        var left = upper - GoldenRatio * (upper - lower);
        var right = lower + GoldenRatio * (upper - lower);

        var leftValue = DistanceSquared(left);
        var rightValue = DistanceSquared(right);

        for (var i = 0; i < 100 && upper - lower > 0.001; i++)
            if (leftValue > rightValue)
            {
                lower = left;
                left = right;
                leftValue = rightValue;

                right = lower + GoldenRatio * (upper - lower);
                rightValue = DistanceSquared(right);
            }
            else
            {
                upper = right;
                right = left;
                rightValue = leftValue;

                left = upper - GoldenRatio * (upper - lower);
                leftValue = DistanceSquared(left);
            }

        var distanceAlongRunway = (lower + upper) / 2.0;
        var corrected = PositionAt(distanceAlongRunway);

        if (corrected is null)
            return null;

        var offset = Inverse(nav, corrected.Value);

        if (offset is null)
            return null;

        return (corrected.Value, offset.Value.Distance);
    }

    /// <summary>
    ///     Calculates ellipsoidal distance and initial true bearing
    ///     between two geographic positions using Vincenty's inverse formula.
    /// </summary>
    public static (double Distance, double Bearing)?
        Inverse(GeoCoordinate first, GeoCoordinate second)
    {
        if (first == second)
            return (0.0, 0.0);

        var phi1 = first.Latitude * DegreesToRadians;
        var phi2 = second.Latitude * DegreesToRadians;

        var reduced1 = Math.Atan(
            (1.0 - Flattening) * Math.Tan(phi1));

        var reduced2 = Math.Atan(
            (1.0 - Flattening) * Math.Tan(phi2));

        var sinU1 = Math.Sin(reduced1);
        var cosU1 = Math.Cos(reduced1);
        var sinU2 = Math.Sin(reduced2);
        var cosU2 = Math.Cos(reduced2);

        var longitudeDifference = NormalizeSignedHeading(
            second.Longitude - first.Longitude) * DegreesToRadians;

        var lambda = longitudeDifference;

        for (var i = 0; i < MaxIterations; i++)
        {
            var sinLambda = Math.Sin(lambda);
            var cosLambda = Math.Cos(lambda);

            var sinSigma = Math.Sqrt(
                Math.Pow(cosU2 * sinLambda, 2) +
                Math.Pow(
                    cosU1 * sinU2 -
                    sinU1 * cosU2 * cosLambda, 2));

            if (sinSigma == 0.0)
                return (0.0, 0.0);

            var cosSigma =
                sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;

            var sigma = Math.Atan2(sinSigma, cosSigma);

            var sinAlpha =
                cosU1 * cosU2 * sinLambda / sinSigma;

            var cosSqAlpha = 1.0 - sinAlpha * sinAlpha;

            var cos2SigmaM = cosSqAlpha > 1e-15
                ? cosSigma - 2.0 * sinU1 * sinU2 / cosSqAlpha
                : 0.0;

            var c = Flattening / 16.0 * cosSqAlpha *
                    (4.0 + Flattening *
                        (4.0 - 3.0 * cosSqAlpha));

            var previous = lambda;

            lambda = longitudeDifference +
                     (1.0 - c) * Flattening * sinAlpha *
                     (sigma + c * sinSigma *
                         (cos2SigmaM + c * cosSigma *
                             (-1.0 + 2.0 *
                                 cos2SigmaM * cos2SigmaM)));

            if (!double.IsFinite(lambda))
                return null;

            if (Math.Abs(lambda - previous) < ConvergenceTolerance)
            {
                var uSq = cosSqAlpha *
                          (SemiMajorAxis * SemiMajorAxis -
                           SemiMinorAxis * SemiMinorAxis) /
                          (SemiMinorAxis * SemiMinorAxis);

                var a = 1.0 + uSq / 16384.0 *
                    (4096.0 + uSq *
                        (-768.0 + uSq * (320.0 - 175.0 * uSq)));

                var b = uSq / 1024.0 *
                        (256.0 + uSq *
                            (-128.0 + uSq * (74.0 - 47.0 * uSq)));

                var deltaSigma = b * sinSigma *
                                 (cos2SigmaM + b / 4.0 *
                                     (cosSigma *
                                      (-1.0 + 2.0 * cos2SigmaM * cos2SigmaM) -
                                      b / 6.0 * cos2SigmaM *
                                      (-3.0 + 4.0 * sinSigma * sinSigma) *
                                      (-3.0 + 4.0 * cos2SigmaM * cos2SigmaM)));

                var distance =
                    SemiMinorAxis * a * (sigma - deltaSigma);

                var bearing = Math.Atan2(
                    cosU2 * Math.Sin(lambda),
                    cosU1 * sinU2 -
                    sinU1 * cosU2 * Math.Cos(lambda));

                return (
                    distance,
                    NormalizeHeading(bearing * RadiansToDegrees));
            }
        }

        // Vincenty's method may fail near antipodal positions.
        return null;
    }

    /// <summary>
    ///     Calculates the destination position along a WGS84 geodesic.
    /// </summary>
    public static GeoCoordinate? Direct(
        GeoCoordinate start,
        double bearing,
        double distance)
    {
        // A negative distance means travel along the reverse bearing.
        if (distance < 0)
        {
            distance = -distance;
            bearing = NormalizeHeading(bearing + 180.0);
        }

        var alpha1 = bearing * DegreesToRadians;
        var phi1 = start.Latitude * DegreesToRadians;
        var lambda1 = start.Longitude * DegreesToRadians;

        var tanU1 = (1.0 - Flattening) * Math.Tan(phi1);
        var cosU1 = 1.0 / Math.Sqrt(1.0 + tanU1 * tanU1);
        var sinU1 = tanU1 * cosU1;

        var sinAlpha1 = Math.Sin(alpha1);
        var cosAlpha1 = Math.Cos(alpha1);

        var sigma1 = Math.Atan2(tanU1, cosAlpha1);

        var sinAlpha = cosU1 * sinAlpha1;
        var cosSqAlpha = 1.0 - sinAlpha * sinAlpha;

        var uSq = cosSqAlpha *
                  (SemiMajorAxis * SemiMajorAxis -
                   SemiMinorAxis * SemiMinorAxis) /
                  (SemiMinorAxis * SemiMinorAxis);

        var a = 1.0 + uSq / 16384.0 *
            (4096.0 + uSq *
                (-768.0 + uSq * (320.0 - 175.0 * uSq)));

        var b = uSq / 1024.0 *
                (256.0 + uSq *
                    (-128.0 + uSq * (74.0 - 47.0 * uSq)));

        var sigma = distance / (SemiMinorAxis * a);
        var converged = false;

        for (var i = 0; i < MaxIterations; i++)
        {
            var cos2SigmaM = Math.Cos(2.0 * sigma1 + sigma);
            var sinSigma = Math.Sin(sigma);
            var cosSigma = Math.Cos(sigma);

            var deltaSigma = b * sinSigma *
                             (cos2SigmaM + b / 4.0 *
                                 (cosSigma *
                                  (-1.0 + 2.0 * cos2SigmaM * cos2SigmaM) -
                                  b / 6.0 * cos2SigmaM *
                                  (-3.0 + 4.0 * sinSigma * sinSigma) *
                                  (-3.0 + 4.0 * cos2SigmaM * cos2SigmaM)));

            var nextSigma =
                distance / (SemiMinorAxis * a) + deltaSigma;

            if (Math.Abs(nextSigma - sigma) < ConvergenceTolerance)
            {
                sigma = nextSigma;
                converged = true;
                break;
            }

            sigma = nextSigma;
        }

        if (!converged)
            return null;

        var sinFinalSigma = Math.Sin(sigma);
        var cosFinalSigma = Math.Cos(sigma);

        var tmp =
            sinU1 * sinFinalSigma -
            cosU1 * cosFinalSigma * cosAlpha1;

        var latitude = Math.Atan2(
            sinU1 * cosFinalSigma +
            cosU1 * sinFinalSigma * cosAlpha1,
            (1.0 - Flattening) *
            Math.Sqrt(sinAlpha * sinAlpha + tmp * tmp));

        var lambda = Math.Atan2(
            sinFinalSigma * sinAlpha1,
            cosU1 * cosFinalSigma -
            sinU1 * sinFinalSigma * cosAlpha1);

        var c = Flattening / 16.0 * cosSqAlpha *
                (4.0 + Flattening *
                    (4.0 - 3.0 * cosSqAlpha));

        var cos2FinalSigmaM = Math.Cos(2.0 * sigma1 + sigma);

        var longitudeCorrection =
            (1.0 - c) * Flattening * sinAlpha *
            (sigma + c * sinFinalSigma *
                (cos2FinalSigmaM + c * cosFinalSigma *
                    (-1.0 + 2.0 *
                        cos2FinalSigmaM * cos2FinalSigmaM)));

        var longitude =
            lambda1 + lambda - longitudeCorrection;

        var result = new GeoCoordinate(
            latitude * RadiansToDegrees,
            NormalizeSignedHeading(longitude * RadiansToDegrees));

        return IsValidCoordinate(result) ? result : null;
    }

    private static double NormalizeHeading(double heading)
    {
        var normalized = heading % 360.0;
        return normalized < 0 ? normalized + 360.0 : normalized;
    }

    private static double NormalizeSignedHeading(double heading)
    {
        var normalized = NormalizeHeading(heading);
        return normalized >= 180.0 ? normalized - 360.0 : normalized;
    }

    private static bool IsValidCoordinate(GeoCoordinate coordinate)
    {
        return double.IsFinite(coordinate.Latitude) &&
               double.IsFinite(coordinate.Longitude) &&
               coordinate.Latitude is >= -90 and <= 90 &&
               coordinate.Longitude is >= -180 and <= 180;
    }
}