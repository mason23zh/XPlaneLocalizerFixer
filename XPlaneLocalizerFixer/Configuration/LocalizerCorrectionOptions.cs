namespace XPlaneLocalizerFixer.Configuration;

/// <summary>
///     Config safety limits applied when correcting localizer alignment
/// </summary>
public sealed class LocalizerCorrectionOptions
{
    public const string SectionName = "LocalizerCorrection";

    /// <summary>
    ///     Maximum lateral distance in the meters that a localizer
    ///     maybe moved during the correction
    /// </summary>
    public double MaxLateralDeviationMeters { get; set; } = 50.0;

    /// <summary>
    ///     Maximum allowed difference in degrees between the localizer course
    ///     and runway heading.
    /// </summary>
    /// <remarks>
    ///     This is to prevent accidentally correct the runway with offset localizer
    ///     For example: ZYTL ILS 10
    /// </remarks>
    public double MaxHeadingDeviationDegrees { get; set; } = 1.0;
}