namespace XPlaneLocalizerFixer.Models;

/// <summary>
///     Represents the navaid records extracted from the Xplane earth_nav.dat file.
/// </summary>
/// <remarks>
///     Group localizers, glideslopes, and DME into separate collections for easier lookup.
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