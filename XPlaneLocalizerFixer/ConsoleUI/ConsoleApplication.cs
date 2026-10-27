using Microsoft.Extensions.Options;
using Spectre.Console;
using XPlaneLocalizerFixer.Configuration;
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
                new Rule("[bold cyan] X-Plane 12 Localizer Fixer[/]"));

            AnsiConsole.WriteLine();

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select an [green]option[/]:")
                    .AddChoices(
                        "Generate corrected nav data",
                        "View configuration",
                        "Exit"));

            AnsiConsole.WriteLine();

            switch (selection)
            {
                case "Generate corrected nav data":
                    GenerateNavData();
                    break;

                case "View configuration":
                    ShowConfiguration();
                    break;

                case "Exit":
                    return;
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

        try
        {
            var result = _workflow.Execute(outputPath);

            AnsiConsole.MarkupLine(
                $"[green]Successfully corrected " +
                $"{result.CorrectedCount} localizers.[/]");

            if (result.OutputPath is null)
            {
                AnsiConsole.MarkupLine(
                    "[yellow]No corrections were generated.[/]");

                return;
            }

            AnsiConsole.MarkupLine(
                $"Generated file: [cyan]" +
                $"{Markup.Escape(result.OutputPath)}[/]");
        }
        catch (Exception ex) when (
            ex is IOException or UnauthorizedAccessException
                or InvalidOperationException or ArgumentException)
        {
            AnsiConsole.MarkupLine(
                $"[red]Error:[/] {Markup.Escape(ex.Message)}[/])");
        }
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
}