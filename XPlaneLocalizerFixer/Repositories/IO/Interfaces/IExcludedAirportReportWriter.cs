using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Repositories.IO.Interfaces;

/// <summary>
///     Defines a writer to create a report to display list of airports that been excluded from the
///     localizer correction.
/// </summary>
public interface IExcludedAirportReportWriter
{
    /// <summary>
    ///     Generate a report containing excluded airport ICAO
    /// </summary>
    /// <param name="outputPath">
    ///     The destination file path.
    /// </param>
    /// <param name="airportIdentifiers">
    ///     The airport identifiers excluded from correction
    /// </param>
    /// <param name="rejections">
    ///     Localizer correction results containing rejection reasons.
    /// </param>
    void Write(
        string outputPath,
        IReadOnlySet<string> airportIdentifiers,
        IReadOnlyList<LocalizerCorrectionResult> rejections);
}