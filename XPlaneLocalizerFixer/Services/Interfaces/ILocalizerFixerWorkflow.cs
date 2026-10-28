using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Services.Interfaces;

/// <summary>
///     Execute the localizer correction processing and nav data generation.
///     Also skip the airport in custom scenery package.
/// </summary>
public interface ILocalizerFixerWorkflow
{
    /// <summary>
    ///     Process eligible localizers and writes their corrections to the specified output file.
    /// </summary>
    /// <param name="xplaneRootPath">
    ///     The Xplane installation directory
    /// </param>
    /// <param name="outputPath">
    ///     Destination path for the generated nav data file.
    /// </param>
    /// <param name="reportPath">
    ///     Destination path for generated airport excluded list file.
    /// </param>
    /// <returns>
    ///     The correction count and generated file path.
    /// </returns>
    LocalizerFixerWorkflowResult Execute(
        string xplaneRootPath,
        string outputPath,
        string reportPath
    );
}