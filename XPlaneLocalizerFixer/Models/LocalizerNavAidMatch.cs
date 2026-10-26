namespace XPlaneLocalizerFixer.Models;

/// <summary>
///     Represents a localizer and its associated glideslope and DME records.
/// </summary>
/// <remarks>
///     Associated navaid reference original records from earth_nav.dat.
/// </remarks>
public sealed class LocalizerNavAidMatch
{
    /// <summary>
    ///     Localizer used to id the associated navaids.
    /// </summary>
    public required Localizer Localizer { get; set; }

    /// <summary>
    ///     Associated glideslope if available.
    /// </summary>
    public GlideSlope? GlideSlope { get; set; }

    /// <summary>
    ///     Associated DME if available.
    /// </summary>
    public Dme? Dme { get; set; }

    /// <summary>
    ///     Indicate whether the localizer is an ILS localizer.
    /// </summary>
    /// <remarks>
    ///     RowCode 4 in earth_nav.dat indicate the localizer is paired with ILS,
    ///     RowCode 5 means stand-alone localizer.
    ///     For now, we only correct ILS paired localizer.
    /// </remarks>
    public bool IsEligibleForCorrection =>
        Localizer.RowCode == 4 && GlideSlope is not null;
}