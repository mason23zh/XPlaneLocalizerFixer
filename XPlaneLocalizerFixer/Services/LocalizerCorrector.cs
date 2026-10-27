using Microsoft.Extensions.Options;
using XPlaneLocalizerFixer.Configuration;
using XPlaneLocalizerFixer.Geometry;
using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Services;

public class LocalizerCorrector : ILocalizerCorrector
{
    private readonly LocalizerCorrectionOptions _options;

    public LocalizerCorrector(IOptions<LocalizerCorrectionOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    public LocalizerCorrectionResult Correct(LocalizerRunwayMatch match)
    {
        ArgumentNullException.ThrowIfNull(match);

        var localizer = match.Localizer;
        var runway = match.Runway;

        var start = runway.StartPosition;
        var end = runway.EndPosition;
        var nav = localizer.Position;

        if (!IsValidCoordinate(start) || !IsValidCoordinate(end))
            return Skipped(localizer, "Invalid runway coordinates.");

        if (!IsValidCoordinate(nav))
            return Skipped(localizer, "Invalid localizer coordinates.");

        if (!double.IsFinite(localizer.EncodedCourse) ||
            localizer.EncodedCourse < 0)
            return Skipped(localizer, "Invalid localizer course.");

        // Determine the runway's initial true bearing on the WGS84 ellipsoid.
        var runwayPath = Wgs84Geodesic.Inverse(start, end);

        if (runwayPath is null || runwayPath.Value.Distance < 1.0)
            return Skipped(localizer, "Invalid runway geometry.");

        var runwayHeading = runwayPath.Value.Bearing;

        // Xplane combines magnetic front course and true bearing.
        var localizerHeading = GeoMath.NormalizeHeading(localizer.EncodedCourse);

        var headingDifference = GeoMath.AngularDifference(
            localizerHeading,
            runwayHeading);

        // Protect intentionally offset localizer approaches.
        if (headingDifference >= _options.MaxHeadingDeviationDegrees)
            return Skipped(
                localizer,
                "Localizer heading differs from runway heading by at least 1 degree.");

        // Find the closest position on the extended runway centerline.
        var projection = Wgs84Geodesic.ProjectOntoRunway(
            start,
            runwayHeading,
            runwayPath.Value.Distance,
            nav);

        if (projection is null)
            return Skipped(localizer, "Unable to project localizer.");

        var (correctedPosition, lateralDisplacement) = projection.Value;

        if (!IsValidCoordinate(correctedPosition) ||
            !double.IsFinite(lateralDisplacement) ||
            lateralDisplacement < 0)
            return Skipped(
                localizer,
                "Invalid geodesic projection result.");

        if (lateralDisplacement > _options.MaxLateralDeviationMeters)
            return Skipped(
                localizer,
                $"Lateral displacement exceeds {_options.MaxLateralDeviationMeters} metres.",
                lateralDisplacement);

        // Preserve the integer magnetic-course portion of X-Plane's encoding.
        var magneticPrefix =
            Math.Floor(localizer.EncodedCourse / 360.0) * 360.0;

        var correctedEncodedCourse =
            magneticPrefix + runwayHeading;

        return new LocalizerCorrectionResult
        {
            Localizer = localizer,
            IsCorrected = true,
            CorrectedLatitude = correctedPosition.Latitude,
            CorrectedLongitude = correctedPosition.Longitude,
            CorrectedEncodedCourse = correctedEncodedCourse,
            LateralDisplacementMeters = lateralDisplacement
        };
    }

    private static LocalizerCorrectionResult Skipped(
        Localizer localizer,
        string reason,
        double? displacement = null)
    {
        return new LocalizerCorrectionResult
        {
            Localizer = localizer,
            IsCorrected = false,
            SkipReason = reason,
            LateralDisplacementMeters = displacement
        };
    }

    private static bool IsValidCoordinate(GeoCoordinate coordinate)
    {
        return double.IsFinite(coordinate.Latitude) &&
               double.IsFinite(coordinate.Longitude) &&
               coordinate.Latitude is >= -90 and <= 90 &&
               coordinate.Longitude is >= -180 and <= 180;
    }
}