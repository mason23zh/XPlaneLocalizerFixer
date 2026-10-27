using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Parsers;

/// <summary>
///     Defines a parser for XPlane scenery pack files
/// </summary>
public interface ISceneryPacksIniParser
{
    /// <summary>
    ///     Reads scenery package entries from the config file
    /// </summary>
    /// <param name="path">
    ///     The scenery_packs.ini file path
    /// </param>
    /// <returns>
    ///     The scenery packages in configured loading order.
    /// </returns>
    IReadOnlyList<SceneryPack> Parse(string path);
}