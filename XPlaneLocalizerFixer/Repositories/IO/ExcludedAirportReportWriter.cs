using System.Globalization;
using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Repositories.IO.Interfaces;

namespace XPlaneLocalizerFixer.Repositories.IO;

public class ExcludedAirportReportWriter : IExcludedAirportReportWriter
{
    private readonly IFileWriter _fileWriter;

    public ExcludedAirportReportWriter(IFileWriter fileWriter)
    {
        _fileWriter = fileWriter;
    }

    public void Write(
        string outputPath,
        IReadOnlySet<string> airportIdentifiers,
        IReadOnlyList<LocalizerCorrectionResult> rejections)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(airportIdentifiers);
        ArgumentNullException.ThrowIfNull(rejections);

        var sortedAirports = airportIdentifiers
            .OrderBy(icao => icao, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var sortedRejections = rejections
            .Where(result => !result.IsCorrected)
            .OrderBy(result => result.Localizer.AirportIcao, StringComparer.OrdinalIgnoreCase)
            .ThenBy(result => result.Localizer.RunwayIdentifier, StringComparer.OrdinalIgnoreCase)
            .ThenBy(result => result.Localizer.Identifier, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var lines = new List<string>
        {
            "# X-Plane Localizer Fixer",
            "# Airports excluded from localizer correction",
            "",
            "# Custom scenery exclusions",
            "# Reason: Enabled custom scenery with runway definitions",
            ""
        };

        lines.AddRange(sortedAirports);

        lines.Add("");
        lines.Add("# Localizer correction rejections");
        lines.Add("");

        foreach (var rejection in sortedRejections)
        {
            var localizer = rejection.Localizer;

            lines.Add(
                $"{localizer.AirportIcao} RWY {localizer.RunwayIdentifier} " +
                $"({localizer.Identifier})");

            lines.Add(
                $"  Reason: {rejection.SkipReason ?? rejection.FailureReason?.ToString() ?? "Unknown"}");

            if (rejection.HeadingDeviationDegrees is double headingDeviation)
                lines.Add(
                    $"  Heading deviation: {headingDeviation.ToString("F3", CultureInfo.InvariantCulture)}°");

            if (rejection.LateralDisplacementMeters is double displacement)
                lines.Add(
                    $"  Lateral displacement: {displacement.ToString("F2", CultureInfo.InvariantCulture)} m");

            lines.Add("");
        }

        lines.Add($"# Total custom scenery airports: {sortedAirports.Length}");
        lines.Add($"# Total rejected localizers: {sortedRejections.Length}");


        _fileWriter.WriteLines(outputPath, lines);
    }
}