namespace XPlaneLocalizerFixer.Models;

/// <summary>
/// Represent ILS glideslope record defined in the X-Plane custom data/earth_nav.dat file.
/// </summary>
/// <remarks>
/// Model this because the user_nav.data required both glideslope and localizer data after only changing the
/// localizer. 
/// </remarks>
public sealed class GlideSlope:NavAid
{
   /// <summary>
   /// Associated runway number
   /// </summary>
   public required string RunwayIdentifier { get; init; } 
   
   /// <summary>
   /// Navaid description
   /// </summary>
   /// <example>
   /// Name
   /// </example>
   public required string Name { get; init; }
   
   /// <summary>
   /// Original encoded glideslope angle and localizer bearing.
   /// </summary>
   /// <remarks>
   /// Associated localizer bearing in true degrees prefixed by glideslope angle times 100,000 and
   /// add heading.
   /// For example, glideslope of 3.25 degrees on heading of 123.456 becomes 325123.456
   /// </remarks>
   public required double EncodingBearing { get; init; }
}