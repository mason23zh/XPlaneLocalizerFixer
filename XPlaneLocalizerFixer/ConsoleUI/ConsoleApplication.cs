using Microsoft.Extensions.Options;
using Spectre.Console;
using XPlaneLocalizerFixer.Configuration;
using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Services;
using XPlaneLocalizerFixer.Services.Interfaces;

namespace XPlaneLocalizerFixer.ConsoleUI;

/// <summary>
///     Console UI using Spectre.Console
/// </summary>
public sealed class ConsoleApplication
{
    private readonly LocalizerCorrectionOptions _correctionOptions;
    private readonly FilePathOptions _fileOptions;
    private readonly IXPlaneInstallationFolderPicker _folderPicker;
    private readonly IXPlaneInstallationValidator _installationValidator;
    private readonly ILocalizerFixerWorkflow _workflow;

    private string? _xplaneRootPath;

    public ConsoleApplication(
        ILocalizerFixerWorkflow workflow,
        IOptions<FilePathOptions> fileOptions,
        IOptions<LocalizerCorrectionOptions> correctionOptions,
        IXPlaneInstallationFolderPicker folderPicker,
        IXPlaneInstallationValidator installationValidator
    )
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(fileOptions);
        ArgumentNullException.ThrowIfNull(correctionOptions);
        ArgumentNullException.ThrowIfNull(folderPicker);
        ArgumentNullException.ThrowIfNull(installationValidator);

        _workflow = workflow;
        _fileOptions = fileOptions.Value;
        _correctionOptions = correctionOptions.Value;
        _folderPicker = folderPicker;
        _installationValidator = installationValidator;
    }

    public void Run()
    {
        while (true)
        {
            AnsiConsole.Clear();

            AnsiConsole.Write(
                new Rule("[bold cyan]X-Plane 12 Localizer Fixer[/]"));

            AnsiConsole.WriteLine();

            if (_xplaneRootPath is not null)
            {
                AnsiConsole.MarkupLine(
                    $"[green]X-Plane installation:[/] {Markup.Escape(_xplaneRootPath)}");

                AnsiConsole.WriteLine();
            }

            var choices = _xplaneRootPath is null
                ? new[]
                {
                    "Select X-Plane 12 installation",
                    "View configuration",
                    "Exit"
                }
                : new[]
                {
                    "Generate corrected nav data",
                    "View configuration",
                    "Change X-Plane installation",
                    "Exit"
                };

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an [green]option[/]:")
                    .AddChoices(choices));

            if (selection == "Exit")
                return;

            AnsiConsole.WriteLine();

            try
            {
                switch (selection)
                {
                    case "Select X-Plane 12 installation":
                    case "Change X-Plane installation":
                        SelectXPlaneInstallation();
                        continue;

                    case "Generate corrected nav data":
                        GenerateNavData();
                        break;

                    case "View configuration":
                        ShowConfiguration();
                        break;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine(
                "[grey]Press any key to return to the menu...[/]");

            Console.ReadKey(true);
        }
    }

    /// <summary>
    ///     Executes localizer correction and display result.
    /// </summary>
    private void GenerateNavData()
    {
        var outputDirectory = Path.GetFullPath(
            _fileOptions.OutputDirectory,
            AppContext.BaseDirectory);

        var outputPath = Path.Combine(
            outputDirectory,
            _fileOptions.OutputFileName);

        var reportPath = Path.Combine(
            outputDirectory,
            _fileOptions.ExcludedAirportsFileName);

        if (_xplaneRootPath is null)
            throw new InvalidOperationException("No X-Plane installation has been selected.");

        LocalizerFixerWorkflowResult? result = null;

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .Start(
                "Processing localizer corrections...",
                _ => { result = _workflow.Execute(_xplaneRootPath, outputPath, reportPath); });

        if (result is null)
            throw new InvalidOperationException(
                "The localizer correction workflow did not return a result.");

        AnsiConsole.MarkupLine(
            $"[green]Successfully corrected {result.CorrectedCount} localizers.[/]");

        if (result.OutputPath is null)
        {
            AnsiConsole.MarkupLine(
                "[yellow]No corrections were generated.[/]");
            return;
        }

        AnsiConsole.MarkupLine("Generated file:");
        AnsiConsole.WriteLine(reportPath);
        AnsiConsole.WriteLine(result.OutputPath);
    }

    /// <summary>
    ///     Displays the configured file locations and localizer correction safety limits.
    /// </summary>
    private void ShowConfiguration()
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Setting")
            .AddColumn("Value");

        table.AddRow(
            "X-Plane installation",
            Markup.Escape(_xplaneRootPath ?? "Not selected"));

        table.AddRow(
            "Output directory",
            Markup.Escape(_fileOptions.OutputDirectory));

        table.AddRow(
            "Output file",
            Markup.Escape(_fileOptions.OutputFileName));

        table.AddRow(
            "Excluded airports report",
            Markup.Escape(_fileOptions.ExcludedAirportsFileName));

        table.AddRow(
            "Max lateral deviation",
            $"{_correctionOptions.MaxLateralDeviationMeters} m");

        table.AddRow(
            "Max heading deviation",
            $"{_correctionOptions.MaxHeadingDeviationDegrees}°");

        AnsiConsole.Write(table);
    }

    private static void HandleException(Exception exception)
    {
        AnsiConsole.WriteLine();

        switch (exception)
        {
            case FileNotFoundException ex:
                AnsiConsole.MarkupLine(
                    "[red]Required input file was not found.[/]");

                if (!string.IsNullOrWhiteSpace(ex.FileName))
                    AnsiConsole.WriteLine(ex.FileName);

                break;

            case DirectoryNotFoundException ex:
                AnsiConsole.MarkupLine(
                    "[red]A required directory was not found.[/]");

                AnsiConsole.WriteLine(ex.Message);
                break;

            case UnauthorizedAccessException ex:
                AnsiConsole.MarkupLine(
                    "[red]Access denied.[/]");

                AnsiConsole.WriteLine(ex.Message);
                break;

            case IOException ex:
                AnsiConsole.MarkupLine(
                    "[red]A file operation failed.[/]");

                AnsiConsole.WriteLine(ex.Message);
                break;

            case ArgumentException ex:
                AnsiConsole.MarkupLine(
                    "[red]Invalid application configuration or input.[/]");

                AnsiConsole.WriteLine(ex.Message);
                break;

            default:
                AnsiConsole.MarkupLine(
                    "[red bold]An unexpected error occurred.[/]");

                AnsiConsole.WriteException(
                    exception,
                    ExceptionFormats.ShortenPaths);

                break;
        }
    }

    /// <summary>
    ///     Prompt the user to select and validate an XPlnae 12 installation directory
    /// </summary>
    /// <returns></returns>
    private bool SelectXPlaneInstallation()
    {
        while (true)
        {
            AnsiConsole.MarkupLine(
                "[cyan]Select your X-Plane 12 installation folder.[/]");

            var selectedPath = _folderPicker.SelectFolder();

            if (selectedPath is null)
                return false;

            if (_installationValidator.IsValidInstallation(selectedPath))
            {
                _xplaneRootPath = selectedPath;

                AnsiConsole.MarkupLine(
                    "[green]X-Plane installation validated successfully.[/]");

                AnsiConsole.WriteLine(selectedPath);
                return true;
            }

            AnsiConsole.MarkupLine(
                "[red]Invalid X-Plane installation directory.[/]");

            if (!AnsiConsole.Confirm("Would you like to try again?"))
                return false;
        }
    }
}