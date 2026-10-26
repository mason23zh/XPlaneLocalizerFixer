using System.Globalization;
using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Utils;

namespace XPlaneLocalizerFixer.Parsers;

/// <summary>
///     Parse navaid records from the XPlane earth_nav.dat file
/// </summary>
/// <remarks>
///     Original GS and DME data are preserved to support the generation of user_nav.dat
/// </remarks>
public sealed class EarthNavDatParser : IEarthNavDatParser
{
    private const int NAV1200_Localizer_Field_Count = 12;
    private const int NAV1200_Glideslope_Field_Count = 12;
    private const int NAV1200_Dme_Field_Count = 11;


    public NavData Parse(IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var navData = new NavData();
        var lineNumber = 0;

        foreach (var line in lines)
        {
            lineNumber++;

            var fields = line.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length == 0 ||
                !int.TryParse(
                    fields[0],
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var rowCode))
                // skip headers;
                continue;

            if (rowCode == 99) break;

            switch (rowCode)
            {
                // both row 4 and 5 are for localizers
                // we keep both, and we will filter out stand-alone localizer 
                case 4:
                case 5:
                {
                    navData.Localizers.Add(
                        ParseLocalizer(fields, line));
                    break;
                }
                case 6:
                {
                    navData.GlideSlopes.Add(
                        ParseGlideSlope(fields, line));
                    break;
                }
                case 12: //only parsing row 12 for now (paried DMEs only)
                {
                    navData.Dmes.Add(
                        ParseDme(fields, line));
                    break;
                }
            }
        }

        return navData;
    }

    /// <summary>
    ///     Parses ILS or standalone localizer record.
    /// </summary>
    /// <param name="fields">
    ///     navaid record field.
    /// </param>
    /// <param name="rawLine">
    ///     Original navaid record
    /// </param>
    /// <returns></returns>
    private static Localizer ParseLocalizer(
        string[] fields,
        string rawLine)
    {
        ValidateFieldCount(fields, NAV1200_Localizer_Field_Count);

        return new Localizer
        {
            RowCode = ParsingUtils.ParseInteger(fields[0]),
            Position = ParsingUtils.ParsePosition(fields),
            ElevationFeet = ParsingUtils.ParseInteger(fields[3]),
            Frequency = ParsingUtils.ParseInteger(fields[4]),
            RangeNm = ParsingUtils.ParseInteger(fields[5]),
            EncodedCourse = ParsingUtils.ParseDouble(fields[6]),
            Identifier = fields[7],
            AirportIcao = fields[8],
            RegionCode = fields[9],
            RunwayIdentifier = fields[10],
            Name = string.Join(" ", fields[11..]),
            RawLine = rawLine
        };
    }


    /// <summary>
    ///     Parse ILS glideslope record.
    /// </summary>
    /// <param name="fields">The navaid record fields</param>
    /// <param name="rawLine">The original navaid record</param>
    /// <returns>parsed glideslope</returns>
    public static GlideSlope ParseGlideSlope(
        string[] fields,
        string rawLine)
    {
        ValidateFieldCount(fields, NAV1200_Glideslope_Field_Count);

        return new GlideSlope
        {
            RowCode = 6,
            Position = ParsingUtils.ParsePosition(fields),
            ElevationFeet = ParsingUtils.ParseInteger(fields[3]),
            Frequency = ParsingUtils.ParseInteger(fields[4]),
            RangeNm = ParsingUtils.ParseInteger(fields[5]),
            EncodingBearing = ParsingUtils.ParseDouble(fields[6]),
            Identifier = fields[7],
            AirportIcao = fields[8],
            RegionCode = fields[9],
            RunwayIdentifier = fields[10],
            Name = string.Join(" ", fields[11..]),
            RawLine = rawLine
        };
    }


    /// <summary>
    ///     Parse DME navaid record
    /// </summary>
    /// <param name="fields">The navaid record fields</param>
    /// <param name="rawLine">The original navaid record</param>
    /// <returns></returns>
    public static Dme ParseDme(
        string[] fields,
        string rawLine
    )
    {
        ValidateFieldCount(fields, NAV1200_Dme_Field_Count);

        return new Dme
        {
            RowCode = 12,
            Position = ParsingUtils.ParsePosition(fields),
            ElevationFeet = ParsingUtils.ParseInteger(fields[3]),
            Frequency = ParsingUtils.ParseInteger(fields[4]),
            RangeNm = ParsingUtils.ParseInteger(fields[5]),
            BiasNm = ParsingUtils.ParseDouble(fields[6]),
            Identifier = fields[7],
            AirportIcao = fields[8],
            RegionCode = fields[9],
            Name = string.Join(" ", fields[10..]),
            RawLine = rawLine
        };
    }


    /// <summary>
    ///     Ensure navaid record contains the requried field
    /// </summary>
    /// <param name="fields">The navaid record field</param>
    /// <param name="minimumCount">The minimum number of fields required.</param>
    /// <exception cref="FormatException">
    ///     Throw when a record does not have enough fields.
    /// </exception>
    private static void ValidateFieldCount(
        string[] fields,
        int minimumCount)
    {
        if (fields.Length < minimumCount)
            throw new FormatException(
                $"Invalid navigation reocrd: expected at least {minimumCount}, found {fields.Length}");
    }
}