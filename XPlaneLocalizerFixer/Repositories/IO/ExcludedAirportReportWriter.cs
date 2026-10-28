namespace XPlaneLocalizerFixer.Repositories.IO;

public class ExcludedAirportReportWriter : IExcludedAirportReportWriter
{
    private readonly IFileWriter _fileWriter;

    public ExcludedAirportReportWriter(IFileWriter fileWriter)
    {
        _fileWriter = fileWriter;
    }

    public void Write(string outputPath, IReadOnlySet<string> airportIdentifiers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(airportIdentifiers);

        var sortedAirports = airportIdentifiers
            .OrderBy(icao => icao, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        IEnumerable<string> lines =
        [
            "# X-Plane Localizer Fixer",
            "# Airports excluded from localizer correction",
            "# Reason: Enabled custom scenery with runway definitions",
            "",
            .. sortedAirports,
            "",
            $"# Total excluded airports: {sortedAirports.Length}"
        ];

        _fileWriter.WriteLines(outputPath, lines);
    }
}