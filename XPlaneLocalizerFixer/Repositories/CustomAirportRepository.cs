using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Repositories.IO;

namespace XPlaneLocalizerFixer.Repositories;

/// <summary>
///     Discovers airports with runway definitions supplied by higher-priority
///     custom scenery packages in X-Plane.
/// </summary>
public sealed class CustomAirportRepository : ICustomAirportRepository
{
    private readonly IFileReader _fileReader;

    public CustomAirportRepository(IFileReader fileReader)
    {
        _fileReader = fileReader;
    }

    public IReadOnlySet<string> GetExcludedAirportIdentifiers(
        string xplaneRootPath,
        IReadOnlyList<SceneryPack> sceneryPacks)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xplaneRootPath);
        ArgumentNullException.ThrowIfNull(sceneryPacks);

        var excludedAirports = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        // var globalAirports = sceneryPacks.FirstOrDefault(pack =>
        //     pack.IsEnabled &&
        //     string.Equals(
        //         pack.Path,
        //         "*GLOBAL_AIRPORTS*",
        //         StringComparison.OrdinalIgnoreCase));
        //
        // if (globalAirports is null)
        //     throw new InvalidDataException(
        //         "An enabled *GLOBAL_AIRPORTS* entry was not found in scenery_packs.ini.");

        foreach (var pack in sceneryPacks)
        {
            // for now, we ignore the priority, don't think if priority matters 
            // we will check all apt.dat in Custom Scenery anyway
            if (!pack.IsEnabled ||
                string.Equals(
                    pack.Path,
                    "*GLOBAL_AIRPORTS*",
                    StringComparison.OrdinalIgnoreCase))
                continue;

            var sceneryDirectory = ResolveSceneryPath(
                xplaneRootPath, pack.Path);

            var aptDatPath = Path.Combine(
                sceneryDirectory,
                "Earth nav data",
                "apt.dat");

            if (!File.Exists(aptDatPath))
                continue;

            ReadAirportIdentifiers(aptDatPath, excludedAirports);
        }

        return excludedAirports;
    }

    /// <summary>
    ///     Resolves a scenery ini path relative to the X-Plane installation.
    /// </summary>
    private static string ResolveSceneryPath(
        string xplaneRootPath,
        string sceneryPath)
    {
        var normalizedPath = sceneryPath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        if (Path.IsPathRooted(normalizedPath))
            throw new InvalidDataException(
                $"Unsupported absolute scenery path: {sceneryPath}");

        var root = Path.GetFullPath(xplaneRootPath);
        var resolvedPath = Path.GetFullPath(
            Path.Combine(root, normalizedPath));

        var relativePath = Path.GetRelativePath(root, resolvedPath);

        if (relativePath == ".." ||
            relativePath.StartsWith(
                $"..{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal) ||
            Path.IsPathRooted(relativePath))
            throw new InvalidDataException(
                $"Scenery path escapes the X-Plane directory: {sceneryPath}");

        return resolvedPath;
    }

    /// <summary>
    ///     Extracts airport identifiers for land airports containing runway records.
    /// </summary>
    /// <remarks>
    ///     We check the apt.dat in custom scenery's Earth nav data folder, if contains row 100, we skip this airport
    /// </remarks>
    private void ReadAirportIdentifiers(
        string aptDatPath,
        HashSet<string> excludedAirports)
    {
        string? currentAirport = null;
        var hasRunway = false;

        foreach (var line in _fileReader.ReadLines(aptDatPath))
        {
            var fields = line.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length == 0)
                continue;

            var rowCode = fields[0];

            if (rowCode is "1" or "16" or "17" or "99")
            {
                // Finish the previous airport before processing the next header.
                if (currentAirport is not null && hasRunway)
                    excludedAirports.Add(currentAirport);

                currentAirport = null;
                hasRunway = false;

                if (rowCode == "99")
                    break;

                // Only land airports (row 1) are relevant to our runway rule.
                if (rowCode != "1")
                    continue;

                if (fields.Length < 5)
                    throw new InvalidDataException(
                        $"Invalid airport header in '{aptDatPath}': {line}");

                currentAirport = fields[4];
            }
            else if (rowCode == "100" && currentAirport is not null)
            {
                hasRunway = true;
            }
        }

        // The file may end without an explicit row 99 terminator.
        if (currentAirport is not null && hasRunway)
            excludedAirports.Add(currentAirport);
    }
}