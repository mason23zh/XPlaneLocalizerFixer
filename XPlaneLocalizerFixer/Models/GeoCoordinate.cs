namespace XPlaneLocalizerFixer.Models;

/// <summary>
/// Represents a geographic position using latitude and longitude
/// </summary>
/// <param name="Latitude"></param>
/// <param name="Longitude"></param>
public readonly record struct GeoCoordinate(
    double Latitude,
    double Longitude
);
    