using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Services.Interfaces;

/// <summary>
///     Defines a service that processes ILS localizers and collect successful
///     corrections with their associated navaids.
/// </summary>
public interface ILocalizerCorrectionProcessor
{
    /// <summary>
    ///     Process ILS Localizer that have an associated glideslope
    /// </summary>
    /// <param name="aptDatPath">
    ///     Full path to Xplane apt.dat file
    /// </param>
    /// <param name="earthNavDatPath">
    ///     Full path to Xplane earth_nav.dat file
    /// </param>
    /// <param name="excludedAirportIdentifiers">
    ///     Airport identifiers that must not receive localizer correction
    /// </param>
    /// <returns>
    ///     Successful localizer corrections paired with their original glideslope and DME records
    /// </returns>
    IReadOnlyList<(
        LocalizerCorrectionResult Correction,
        LocalizerNavAidMatch NavAids)> Process(
        string aptDatPath,
        string earthNavDatPath,
        IReadOnlySet<string> excludedAirportIdentifiers
    );
}