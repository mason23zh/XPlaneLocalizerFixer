namespace XPlaneLocalizerFixer.Models;

/// <summary>
///     Represents an airport defined in the X-Plane
///     Global Scenery/Global Airports/Earth nav data/apt.dat
/// </summary>
public class Airport
{
    public required string Identifier { get; init; }

    /// <summary>
    ///     Airport's ICAO code if available.
    /// </summary>
    /// <remarks>
    ///     ICAO code might be missing
    /// </remarks>
    public string? IcaoCode { get; init; }

    /// <summary>
    ///     Airport name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Runway that associated with this airport
    /// </summary>
    public List<Runway> Runways { get; init; } = [];
}