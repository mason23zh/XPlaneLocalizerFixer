namespace XPlaneLocalizerFixer.Models;

/// <summary>
/// Represents navaids defined in the X-Plane Custom Data/earth_nav.data file.
/// </summary>
/// <remarks>
/// https://developer.x-plane.com/wp-content/uploads/2016/10/XP-NAV1200-Spec-2.pdf
/// </remarks>
public class NavAid
{
    /// <summary>
    /// X-Plane navigation record type.
    /// </summary>
    /// <remarks>
    /// 4 - ILS localizer
    /// 5 - Standalone localizer, LDA, or SDF
    /// 6 - GS
    /// 12 - DME
    /// </remarks>
    public required int RowCode { get; init; }
    
    /// <summary>
    /// Geographic coordinates for the navaid.
    /// </summary>
    public required GeoCoordinate Position { get; init; }
    
    
    /// <summary>
    /// Navaid elevation in feet above MSL
    /// </summary>
    public required int ElevationFeet { get; init; }
    
    /// <summary>
    /// Navaid frequency multiplied by 100
    /// </summary>
    /// <remarks>
    /// For example, a frequency of 110.90 mhz is stored as 110900
    /// </remarks>
    public required int Frequency { get; init; }
    
    /// <summary>
    /// Max reception range in NM
    /// </summary>
    public required int RangeNm { get; init; }
    
    /// <summary>
    /// Navaid identifier
    /// </summary>
    /// <remarks>
    /// Localizer and GS usually (or always?) share the same identifier.
    /// </remarks>
    public required string Identifier { get; init; }
    
    /// <summary>
    /// Associated airport ICAO if available.
    /// </summary>
    public string? AirportIcao { get; init; }
    
    /// <summary>
    /// ICAO region code of the navaid
    /// </summary>
   public required string RegionCode { get; init; } 
    
    /// <summary>
    /// Original unmodified navaid record.
    /// </summary>
    public required string RawLine { get; init; }
}