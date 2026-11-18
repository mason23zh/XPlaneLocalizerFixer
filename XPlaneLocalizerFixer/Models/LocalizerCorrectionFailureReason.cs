namespace XPlaneLocalizerFixer.Models;

/// <summary>
///     Identifies why a localizer correction could not be performed.
///     We don't contain LOC only approach because localizer without paired GS will be saliently skipped.
/// </summary>
public enum LocalizerCorrectionFailureReason
{
    HeadingDeviationExceeded,
    LateralDeviationExceeded,
    IdenticalRunwayEndpoints,
    InvalidCoordinates
}