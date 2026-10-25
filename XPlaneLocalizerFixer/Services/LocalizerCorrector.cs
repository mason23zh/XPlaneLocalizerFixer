using XPlaneLocalizerFixer.Geometry;
using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Services;

/// <summary>
///     Calculates localizer position and course corrections using the
///     centerline of the associated runway.
/// </summary>
public sealed class LocalizerCorrector : ILocalizerCorrector
{
    private const double MaxHeadingDifferenceDegrees = 1.0;
    private const double MaxLateralDisplacementMeters = 50.0;

    public LocalizerCorrectionResult Correct(LocalizerRunwayMatch match)
    {
        ArgumentNullException.ThrowIfNull(match);

        var localizer = match.Localizer;
        var runway = match.Runway;

        var threshold = runway.StartPosition;
        var oppositeThreshold = runway.EndPosition;

        var runwayHeading = GeoMath.CalculateBearing(
            threshold.Latitude,
            threshold.Longitude,
            oppositeThreshold.Latitude,
            oppositeThreshold.Longitude);

        // The true bearing is the remainder of the encoded course.
        var localizerHeading =
            GeoMath.NormalizeHeading(localizer.EncodedCourse);

        var headingDifference = GeoMath.AngularDifference(
            localizerHeading,
            runwayHeading);

        if (headingDifference > MaxHeadingDifferenceDegrees)
            return Skipped(
                localizer,
                "Localizer heading differs from runway heading by more than 1 degree.");

        var projection = new GnomonicProjection(
            threshold.Latitude,
            threshold.Longitude);

        var runwayEnd = projection.Project(
            oppositeThreshold.Latitude,
            oppositeThreshold.Longitude);

        var navPosition = projection.Project(
            localizer.Position.Latitude,
            localizer.Position.Longitude);

        // The projection center is the designated runway threshold (0, 0).
        var runwayVector = runwayEnd;

        var runwayLengthSquared = runwayVector.Dot(runwayVector);

        if (!double.IsFinite(runwayLengthSquared) ||
            runwayLengthSquared <= 0)
            return Skipped(localizer, "Invalid runway geometry.");

        // Project the localizer perpendicularly onto the infinite
        // runway centerline, preserving its longitudinal position.
        var factor =
            navPosition.Dot(runwayVector) / runwayLengthSquared;

        var correctedPosition = runwayVector * factor;

        var lateralDisplacement =
            navPosition.DistanceTo(correctedPosition);

        if (!double.IsFinite(lateralDisplacement)) return Skipped(localizer, "Invalid lateral displacement.");

        if (lateralDisplacement > MaxLateralDisplacementMeters)
            return Skipped(
                localizer,
                $"Lateral displacement exceeds {MaxLateralDisplacementMeters} metres.",
                lateralDisplacement);

        var correctedCoordinates =
            projection.Unproject(correctedPosition);

        // Preserve the encoded integer magnetic front-course component.
        var magneticCoursePrefix =
            Math.Floor(localizer.EncodedCourse / 360.0) * 360.0;

        var correctedEncodedCourse =
            magneticCoursePrefix + runwayHeading;

        return new LocalizerCorrectionResult
        {
            Localizer = localizer,
            IsCorrected = true,
            CorrectedLatitude = correctedCoordinates.Latitude,
            CorrectedLongitude = correctedCoordinates.Longitude,
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
}