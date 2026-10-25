using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Parsers;

/// <summary>
///     Deifnes operations for parsing navaid records
///     from the Xplane earth_nav.dat file
/// </summary>
public interface IEarthNavDatParser
{
    /// <summary>
    ///     Parse navaid records from text lines.
    /// </summary>
    /// <param name="lines">
    ///     The soruce line from the earth_nav.dat file.
    /// </param>
    /// <returns>
    ///     Parsed localzer, glideslope and DME collections
    /// </returns>
    NavData Parse(IEnumerable<string> lines);
}