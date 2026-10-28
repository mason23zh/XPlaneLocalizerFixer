using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Parsers.Interfaces;

/// <summary>
///     Defines operations for parsing navaid records
///     from the Xplane earth_nav.dat file
/// </summary>
public interface IEarthNavDatParser
{
    /// <summary>
    ///     Parse navaid records from text lines.
    /// </summary>
    /// <param name="lines">
    ///     The source line from the earth_nav.dat file.
    /// </param>
    /// <returns>
    ///     Parsed localizer, glideslope and DME collections
    /// </returns>
    NavData Parse(IEnumerable<string> lines);
}