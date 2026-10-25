namespace XPlaneLocalizerFixer.Models;

/// <summary>
///     Represents the result of attempting to align a localizer
///     with it's associated runway centerline.
/// </summary>
public sealed record LocalizerCorrectionResult
{
    /// <summary>
    ///     Original localizer
    /// </summary>
    public required Localizer Localizer { get; init; }

    /// <summary>
    ///     Whether the correction was successfully.
    /// </summary>
    public required bool IsCorrected { get; init; }

    /// <summary>
    ///     Gets the corrected longitude in degrees if available.
    /// </summary>
    public double? CorrectedLongitude { get; init; }

    /// <summary>
    ///     Gets the corrected latitude in degrees if available.
    /// </summary>
    public double? CorrectedLatitude { get; init; }

    /// <summary>
    ///     Gets the lateral displacement in meters if available.
    /// </summary>
    public double? LateralDisplacementMeters { get; init; }

    /// <summary>
    ///     Gets the corrected course if available.
    /// </summary>
    public double? CorrectedEncodedCourse { get; init; }

    /// <summary>
    ///     Gets the reason why correction was skipped if available.
    /// </summary>
    public string? SkipReason { get; init; }
}