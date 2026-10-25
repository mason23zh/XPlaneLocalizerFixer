namespace XPlaneLocalizerFixer.Models;

/// <summary>
///     Represents the navad records extraced from the Xplane earth_nav.dat file.
/// </summary>
/// <remarks>
///     Group localizers, glideslopes, and DME into seperate collections for easier lookup.
/// </remarks>
public class NavData
{
    /// <summary>
    ///     ILS Localizers
    /// </summary>
    public List<Localizer> Localizers { get; } = [];

    /// <summary>
    ///     ILS glideslopes
    /// </summary>
    public List<GlideSlope> GlideSlopes { get; } = [];

    /// <summary>
    ///     DMEs
    /// </summary>
    public List<Dme> Dmes { get; } = [];
}