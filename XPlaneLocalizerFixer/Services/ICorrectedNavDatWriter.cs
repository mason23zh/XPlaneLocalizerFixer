using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Services;

/// <summary>
///     Defines a service for generating the Xplane user_nav.dat file containing corrected
///     ILS localizers and their associated navads.
/// </summary>
public interface ICorrectedNavDatWriter
{
    /// <summary>
    ///     Writes successful localizer corrections and their original
    ///     associated navaids to a new nav data file.
    /// </summary>
    /// <param name="outputPath">
    ///     Destination path for the generated data file
    /// </param>
    /// <param name="corrections">
    ///     Successful corrections paired with their original glideslope
    ///     and DME records
    /// </param>
    /// <exception cref="ArgumentException">
    ///     Throw when the output path is empty or whitespace
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     Throw when corrections is null
    /// </exception>
    /// <exception cref="IOException">
    ///     Throw when file already exists
    /// </exception>
    void Write(
        string outputPath,
        IReadOnlyList<(LocalizerCorrectionResult Correction,
            LocalizerNavAidMatch NavAids)> corrections);
}