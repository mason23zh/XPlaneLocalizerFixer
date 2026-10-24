namespace XPlaneLocalizerFixer.Models;

/// <summary>
/// Represents DME navaid record defined in the X-Plane custom data/earth_nav.dat
/// </summary>
/// <remarks>
/// Model this because the user_nav.data required both glideslope and dme data after only changing the
/// localizer. 
/// </remarks>
public class Dme:NavAid
{
   /// <summary>
   /// DME bias in nautical miles
   /// </summary>
   /// <remarks>
   /// Default is 0.0 - Up to one decimal place.
   /// </remarks>
   public required double BiasNm { get; init; } 
   
   /// <summary>
   /// DME name
   /// </summary>
   /// <remarks>
   /// Text suffix with "DME" or "DME-ILS"
   /// Suffix "DME" to navaid name for VOR/DMEs, VORTACs & NDB-DMEs, for standalone DMEs,
   /// use DME name.
   /// </remarks>
   public required string Name { get; init; }
}