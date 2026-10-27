using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Services;

/// <summary>
///     Execute the localizer correction processing and nav data generation.
/// </summary>
public sealed class LocalizerFixerWorkflow : ILocalizerFixerWorkflow
{
    private readonly ILocalizerCorrectionProcessor _processor;
    private readonly ICorrectedNavDatWriter _writer;

    public LocalizerFixerWorkflow(
        ILocalizerCorrectionProcessor processor,
        ICorrectedNavDatWriter writer)
    {
        ArgumentNullException.ThrowIfNull(processor);
        ArgumentNullException.ThrowIfNull(writer);

        _processor = processor;
        _writer = writer;
    }

    public LocalizerFixerWorkflowResult Execute(string outputPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        var corrections = _processor.Process();

        if (corrections.Count == 0)
            return new LocalizerFixerWorkflowResult(0, null);

        var directory = Path.GetDirectoryName(
            Path.GetFullPath(outputPath));

        if (directory is not null)
            Directory.CreateDirectory(directory);

        _writer.Write(outputPath, corrections);

        return new LocalizerFixerWorkflowResult
        (
            corrections.Count,
            outputPath
        );
    }
}