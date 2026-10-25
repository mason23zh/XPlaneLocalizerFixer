using System.Globalization;
using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Utils;

public static class ParsingUtils
{
    /// <summary>
    ///     Parse an integer using the invariant culture.
    /// </summary>
    /// <param name="value">The numeric field</param>
    /// <returns>The parsed integer</returns>
    public static int ParseInteger(string value)
    {
        return int.Parse(
            value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Parse a float number using the invariant culture.
    /// </summary>
    /// <param name="value">The numeric field</param>
    /// <returns>The parsed float</returns>
    public static double ParseDouble(string value)
    {
        return double.Parse(
            value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Extracts coordinates from a navaid record
    /// </summary>
    /// <param name="fields">Navaid record fields</param>
    /// <returns>The geographic position</returns>
    public static GeoCoordinate ParsePosition(string[] fields)
    {
        return new GeoCoordinate(
            ParseDouble(fields[1]),
            ParseDouble(fields[2]));
    }
}