using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Services;

/// <summary>
///     Defines a service that processes ILS localizers and collect successful
///     corrections with their associated navaids.
/// </summary>
public interface ILocalizerCorrectionProcessor
{
    /// <summary>
    ///     Process ILS Localizer that have an associated glideslope
    /// </summary>
    /// <returns>
    ///     Successful localizer corrections paired with their original glideslope and DME records
    /// </returns>
    IReadOnlyList<(
        LocalizerCorrectionResult Correction,
        LocalizerNavAidMatch NavAids)> Process();
}