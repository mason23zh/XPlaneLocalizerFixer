using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Parsers;

/// <summary>
///     Defines operations for parsing airport and runway records
///     from the Xplane apt.dat file
/// </summary>
public interface IAptDatParser
{
    /// <summary>
    ///     Parse airport data from sequence of text lines.
    /// </summary>
    /// <param name="lines">
    ///     The input lines contains Xplane airport records.
    /// </param>
    /// <returns>
    ///     A readonly collection of parsed airports and their runways
    /// </returns>
    IReadOnlyList<Airport> Parse(IEnumerable<string> lines);
}