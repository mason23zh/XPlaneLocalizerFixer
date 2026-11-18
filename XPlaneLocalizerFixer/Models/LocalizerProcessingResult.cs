namespace XPlaneLocalizerFixer.Models;

/// <summary>
///     Represents the results of processing localizers, including
///     successful corrections and rejected corrections.
/// </summary>
public sealed record LocalizerProcessingResult
{
    /// <summary>
    ///     Gets the successfully corrected localizers and
    ///     their associated navigation aids.
    /// </summary>
    public required IReadOnlyList<(
        LocalizerCorrectionResult Correction,
        LocalizerNavAidMatch NavAids)> Corrections { get; init; }

    /// <summary>
    ///     Gets localizers that were rejected during correction.
    /// </summary>
    public required IReadOnlyList<LocalizerCorrectionResult> Rejections { get; init; }
}