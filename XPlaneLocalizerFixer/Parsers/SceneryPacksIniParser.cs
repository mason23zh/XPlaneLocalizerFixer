using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Parsers.Interfaces;
using XPlaneLocalizerFixer.Repositories.IO;
using XPlaneLocalizerFixer.Repositories.IO.Interfaces;

namespace XPlaneLocalizerFixer.Parsers;

public sealed class SceneryPacksIniParser : ISceneryPacksIniParser
{
    private readonly IFileReader _fileReader;

    public SceneryPacksIniParser(IFileReader fileReader)
    {
        _fileReader = fileReader;
    }

    public IReadOnlyList<SceneryPack> Parse(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var sceneryPacks = new List<SceneryPack>();

        foreach (var line in _fileReader.ReadLines(path))
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine) ||
                trimmedLine.StartsWith("#"))
                continue;

            bool isEnabled;
            string sceneryPath;

            if (trimmedLine.StartsWith("SCENERY_PACK_DISABLED", StringComparison.Ordinal))
            {
                isEnabled = false;
                sceneryPath = trimmedLine["SCENERY_PACK_DISABLED".Length..].Trim();
            }
            else if (trimmedLine.StartsWith("SCENERY_PACK", StringComparison.Ordinal))
            {
                isEnabled = true;
                sceneryPath = trimmedLine["SCENERY_PACK".Length..].Trim();
            }
            else
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(sceneryPath))
                continue;

            sceneryPacks.Add(new SceneryPack(
                sceneryPath,
                isEnabled,
                sceneryPacks.Count));
        }

        return sceneryPacks;
    }
}