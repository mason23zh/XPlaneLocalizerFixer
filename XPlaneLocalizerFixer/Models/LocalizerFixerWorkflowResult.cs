namespace XPlaneLocalizerFixer.Models;

/// <summary>
///     Represents the outcome of the localizer correction workflow
/// </summary>
/// <param name="CorrectedCount">
///     Number of successfully corrected localizers
/// </param>
/// <param name="OutputPath">
///     Path to generated nav data file, null when no file was generated.
/// </param>
public sealed record LocalizerFixerWorkflowResult(
    int CorrectedCount,
    string? OutputPath);