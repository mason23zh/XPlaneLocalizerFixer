using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Repositories.Interfaces;

/// <summary>
///     Find airports in custom scenery package.
/// </summary>
public interface ICustomAirportRepository
{
    /// <summary>
    ///     Returns airport identifiers that should be excluded from localizer correction
    /// </summary>
    /// <param name="xplaneRootPath">
    ///     Xplane installation directory
    /// </param>
    /// <param name="sceneryPacks">
    ///     The configured scenery packages
    /// </param>
    /// <returns>
    ///     A set of airport identifiers
    /// </returns>
    IReadOnlySet<string> GetExcludedAirportIdentifiers(
        string xplaneRootPath,
        IReadOnlyList<SceneryPack> sceneryPacks);
}