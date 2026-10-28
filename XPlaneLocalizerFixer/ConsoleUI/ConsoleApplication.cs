using Microsoft.Extensions.Options;
using Spectre.Console;
using XPlaneLocalizerFixer.Configuration;
using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Services;

namespace XPlaneLocalizerFixer.ConsoleUI;

/// <summary>
///     Console UI using Spectre.Console
/// </summary>
public sealed class ConsoleApplication
{
    private readonly LocalizerCorrectionOptions _correctionOptions;
    private readonly FilePathOptions _fileOptions;
    private readonly ILocalizerFixerWorkflow _workflow;

    public ConsoleApplication(
        ILocalizerFixerWorkflow workflow,
        IOptions<FilePathOptions> fileOptions,
        IOptions<LocalizerCorrectionOptions> correctionOptions
    )
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(fileOptions);
        ArgumentNullException.ThrowIfNull(correctionOptions);

        _workflow = workflow;
        _fileOptions = fileOptions.Value;
        _correctionOptions = correctionOptions.Value;
    }

    public void Run()
    {
        while (true)
        {
            AnsiConsole.Clear();

            AnsiConsole.Write(
                new Rule("[bold cyan]X-Plane 12 Localizer Fixer[/]"));

            AnsiConsole.WriteLine();

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an [green]option[/]:")
                    .AddChoices(
                        "Generate corrected nav data",
                        "View configuration",
                        "Exit"));

            if (selection == "Exit")
                return;

            AnsiConsole.WriteLine();

            try
            {
                switch (selection)
                {
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
        if (!ValidateInputFiles())
            return;

        var outputDirectory = Path.GetFullPath(
            _fileOptions.OutputDirectory,
            AppContext.BaseDirectory);

        var outputPath = Path.Combine(
            outputDirectory,
            _fileOptions.OutputFileName);

        var reportPath = Path.Combine(
            outputDirectory,
            _fileOptions.ExcludedAirportsFileName);

        //TODO: Temporarily until we implement user selection
        var xplaneRootPath = @"F:\SteamLibrary\steamapps\common\X-Plane 12";

        LocalizerFixerWorkflowResult? result = null;

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .Start(
                "Processing localizer corrections...",
                _ => { result = _workflow.Execute(xplaneRootPath, outputPath, reportPath); });

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

        table.AddRow("Input directory",
            Markup.Escape(_fileOptions.InputDirectory));

        table.AddRow("Airport data",
            Markup.Escape(_fileOptions.AptDatFileName));

        table.AddRow("Navigation data",
            Markup.Escape(_fileOptions.EarthNavDatFileName));

        table.AddRow("Output directory",
            Markup.Escape(_fileOptions.OutputDirectory));

        table.AddRow("Output file",
            Markup.Escape(_fileOptions.OutputFileName));

        table.AddRow("Max lateral deviation",
            $"{_correctionOptions.MaxLateralDeviationMeters} m");

        table.AddRow("Max heading deviation",
            $"{_correctionOptions.MaxHeadingDeviationDegrees}°");

        AnsiConsole.Write(table);
    }

    /// <summary>
    ///     Check if the required airport and nav data files exist in the configured input directory
    /// </summary>
    /// <returns>
    ///     True when both input files exsits, false otherwise.
    /// </returns>
    private bool ValidateInputFiles()
    {
        var inputDirectory = Path.GetFullPath(
            _fileOptions.InputDirectory,
            AppContext.BaseDirectory);

        var aptDatPath = Path.Combine(
            inputDirectory,
            _fileOptions.AptDatFileName);

        var earthNavDatPath = Path.Combine(
            inputDirectory,
            _fileOptions.EarthNavDatFileName);

        if (!Directory.Exists(inputDirectory))
        {
            AnsiConsole.MarkupLine(
                "[red]Input directory does not exist.[/]");

            AnsiConsole.WriteLine(inputDirectory);
            return false;
        }

        var valid = true;

        if (!File.Exists(aptDatPath))
        {
            AnsiConsole.MarkupLine("[red]Missing apt.dat file:[/]");
            AnsiConsole.WriteLine(aptDatPath);
            valid = false;
        }

        if (!File.Exists(earthNavDatPath))
        {
            AnsiConsole.MarkupLine("[red]Missing earth_nav.dat file:[/]");
            AnsiConsole.WriteLine(earthNavDatPath);
            valid = false;
        }

        return valid;
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
}