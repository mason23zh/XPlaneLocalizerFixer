using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories;
using XPlaneLocalizerFixer.Repositories.IO;

namespace XPlaneLocalizerFixer.Services;

/// <summary>
///     Execute the localizer correction processing and nav data generation.
/// </summary>
public sealed class LocalizerFixerWorkflow : ILocalizerFixerWorkflow
{
    private readonly ICustomAirportRepository _customAirportRepository;
    private readonly ILocalizerCorrectionProcessor _processor;
    private readonly IExcludedAirportReportWriter _reportWriter;
    private readonly ISceneryPacksIniParser _sceneryParser;
    private readonly ICorrectedNavDatWriter _writer;

    public LocalizerFixerWorkflow(
        ILocalizerCorrectionProcessor processor,
        ICorrectedNavDatWriter writer,
        ISceneryPacksIniParser sceneryParser,
        ICustomAirportRepository customAirportRepository,
        IExcludedAirportReportWriter reportWriter)
    {
        ArgumentNullException.ThrowIfNull(processor);
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(sceneryParser);
        ArgumentNullException.ThrowIfNull(customAirportRepository);
        ArgumentNullException.ThrowIfNull(reportWriter);

        _processor = processor;
        _writer = writer;
        _sceneryParser = sceneryParser;
        _customAirportRepository = customAirportRepository;
        _reportWriter = reportWriter;
    }

    public LocalizerFixerWorkflowResult Execute(string xplaneRootPath, string outputPath, string reportPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xplaneRootPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(reportPath);

        var sceneryIniPath = Path.Combine(
            xplaneRootPath,
            "Custom Scenery",
            "scenery_packs.ini");

        // custom scenery detection
        var sceneryPacks = _sceneryParser.Parse(sceneryIniPath);

        var excludedAirports = _customAirportRepository
            .GetExcludedAirportIdentifiers(xplaneRootPath, sceneryPacks);

        var outputDirectory = Path.GetDirectoryName(
            Path.GetFullPath(outputPath));

        if (outputDirectory is null)
            throw new InvalidOperationException("Unable to determine the output directory.");

        Directory.CreateDirectory(outputDirectory);

        _reportWriter.Write(reportPath, excludedAirports);

        // Global Airports ruwnay data
        var aptDatPath = Path.Combine(
            xplaneRootPath,
            "Global Scenery",
            "Global Airports",
            "Earth nav data",
            "apt.dat");

        // custom nav data like Navigraph
        var customNavPath = Path.Combine(
            xplaneRootPath,
            "Custom Data",
            "earth_nav.dat");

        // default nav data shipped with Xplane
        var defaultNavPath = Path.Combine(
            xplaneRootPath,
            "Resources",
            "default data",
            "earth_nav.dat");

        // if user has custom nav data like navigraph, use custom data
        var earthNavDatPath = File.Exists(customNavPath) ? customNavPath : defaultNavPath;

        var corrections = _processor
            .Process(aptDatPath, earthNavDatPath, excludedAirports);

        if (corrections.Count == 0)
        {
            // remove stale data
            if (File.Exists(outputPath))
                File.Delete(outputPath);

            return new LocalizerFixerWorkflowResult(0, null);
        }

        _writer.Write(outputPath, corrections);

        return new LocalizerFixerWorkflowResult
        (
            corrections.Count,
            outputPath
        );
    }
}