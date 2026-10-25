namespace XPlaneLocalizerFixer.Models;

/// <summary>
///     Represents a single runway direction defined in the X-Plane
///     Global Scenery/Global Airports/Earth nav data/apt.dat
/// </summary>
/// <remarks>
///     Runway heading is not included on purpose. Runway heading will be
///     calculated by using StartPosition and EndPosition coordinate.
/// </remarks>
public sealed class Runway
{
    public required string Identifier { get; init; }

    /// <summary>
    ///     Geographic position of runway end coordinate
    /// </summary>
    public required GeoCoordinate StartPosition { get; init; }

    /// <summary>
    ///     Geographic position of the opposite runway end.
    /// </summary>
    public required GeoCoordinate EndPosition { get; init; }
}