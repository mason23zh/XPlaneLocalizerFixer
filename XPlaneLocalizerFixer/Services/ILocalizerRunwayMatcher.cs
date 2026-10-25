using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Services;

public interface ILocalizerRunwayMatcher
{
    /// <summary>
    ///     Match a localizer ot its corresponding airport runway
    /// </summary>
    /// <param name="localizer">The localizer to match</param>
    /// <param name="airports">Airport list loaded from apt.dat</param>
    /// <returns>
    ///     The matching runway info, or null when no match.
    /// </returns>
    LocalizerRunwayMatch? Match(
        Localizer localizer,
        IEnumerable<Airport> airports);
}