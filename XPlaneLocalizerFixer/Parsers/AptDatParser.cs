using System.Globalization;
using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Parsers;

/// <summary>
///     Parse airport adn runway from X-Plane apt.dat file using APT.DAT 1200 specs.
/// </summary>
/// <remarks>
///     Only parse land airport (row code == 1)
///     Prase land runways (row code == 100)
///     Extract airport ICAO metadata (row code == 1302)
///     Other airport records are ignored.
/// </remarks>
public class AptDatParser
{
    /// <summary>
    ///     Parses airport records from sequcne of apt.dat lines.
    /// </summary>
    /// <param name="lines">
    ///     Source line
    /// </param>
    /// <returns>
    ///     A collection of land airports and thier directional runways.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Throw when line is null
    /// </exception>
    /// <exception cref="FormatException">
    ///     Throw when record contains invalid fields
    /// </exception>
    public IReadOnlyList<Airport> Parse(IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var airports = new List<Airport>();

        string? currentIdentifier = null;
        string? currentName = null;
        string? currentIcao = null;

        var currentRunways = new List<Runway>();
        var lineNumber = 0;

        void FinishAirport()
        {
            if (currentIdentifier == null || currentName == null) return;

            var airport = new Airport
            {
                Identifier = currentIdentifier,
                Name = currentName,
                IcaoCode = currentIcao
            };

            airport.Runways.AddRange(currentRunways);
            airports.Add(airport);
        }


        foreach (var line in lines)
        {
            ++lineNumber;

            var fields = line.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length == 0 || fields[0].StartsWith('#')) continue;

            // extract the row code from the first element in the line
            if (!int.TryParse(fields[0], out var rowCode)) continue;

            switch (rowCode)
            {
                // hitting 1,16 or 17 means the new airport appears
                case 1:
                case 16:
                case 17:
                {
                    // Finish process the previous airport
                    FinishAirport();

                    //reset everything for new airprot
                    currentIdentifier = null;
                    currentName = null;
                    currentIcao = null;
                    currentRunways = [];

                    // only parse land airports, ignore other if rowCode != 1
                    if (rowCode != 1) break;

                    /*
                     * Row codes 1, 16 and 16 share a common format
                     * Row codes 1 will have 6 fields
                     */
                    if (fields.Length < 6)
                        throw new FormatException(
                            $"Invalid airprot header at line {lineNumber}");

                    // 5th element in the field is airport identifier
                    currentIdentifier = fields[4];
                    // rest is name
                    currentName = string.Join(" ", fields[5..]);
                    break;
                }
                case 100:
                {
                    if (currentIcao is null) break;

                    ParseRunway(fields, currentRunways, lineNumber);
                    break;
                }
                case 1302:
                {
                    if (currentIdentifier is null) break;

                    if (fields.Length >= 3 && fields[1].Equals(
                            "icao_code",
                            StringComparison.OrdinalIgnoreCase))
                        currentIcao = fields[2];

                    break;
                }
                case 99:
                {
                    FinishAirport();
                    return airports;
                }
            }
        }

        FinishAirport();
        return airports;
    }

    private static void ParseRunway(string[] fields, List<Runway> runways, int lineNumber)
    {
        /*
         * row 100 runway record contains 8 general fields.
         * Start from the 9th field it will repetaed for each end of the runway, since the minumal number of runway
         * is 1, so there will be 2 ends
         * Hence the minumal fields number would be 9 * 2 + 8 = 26
         */
        const int ExpecatedFieldCount = 26;

        if (fields.Length < ExpecatedFieldCount)
            throw new FormatException(
                $"Invalid runway record at line {lineNumber}." +
                $"Expeted {ExpecatedFieldCount} fields but found {fields.Length} fields."
            );


        var firstIdentifier = fields[8];
        var secondIdentifier = fields[17];

        var firstPosition = new GeoCoordinate(
            ParseCoordinate(fields[9]),
            ParseCoordinate(fields[10])
        );

        var secondPosition = new GeoCoordinate(
            ParseCoordinate(fields[18]),
            ParseCoordinate(fields[19])
        );

        /*
         * Each runway has two directions.
         * Reverse the endpoints for its reciprocal direction
         */
        runways.Add(new Runway
        {
            Identifier = firstIdentifier,
            StartPosition = firstPosition,
            EndPosition = secondPosition
        });

        runways.Add(new Runway
        {
            Identifier = secondIdentifier,
            StartPosition = secondPosition,
            EndPosition = firstPosition
        });
    }

    /// <summary>
    ///     Parse the cooridnate string values in the apt.dat to double
    /// </summary>
    /// <param name="value">The cooridante text</param>
    /// <returns>The coordinate in decimal degrees</returns>
    /// <remarks>
    ///     Assume the coordiantes in apt.dat provided by the XPlane should always be valid.
    /// </remarks>
    private static double ParseCoordinate(string value)
    {
        return double.Parse(
            value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture
        );
    }
}