using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Services;

/// <summary>
///     Execute the localizer correction processing and nav data generation.
/// </summary>
public interface ILocalizerFixerWorkflow
{
    /// <summary>
    ///     Process eligible localizers and writes their corrections to the specified output file.
    /// </summary>
    /// <param name="outputPath">
    ///     Destination path for the generated nav data file.
    /// </param>
    /// <returns>
    ///     The correction count and generated file path.
    /// </returns>
    LocalizerFixerWorkflowResult Execute(string outputPath);
}