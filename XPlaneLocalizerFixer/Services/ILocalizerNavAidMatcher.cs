using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Services;

/// <summary>
///     Defines a service for matching localizer with their associated
///     glideslope and DME navaid.
/// </summary>
public interface ILocalizerNavAidMatcher
{
    /// <summary>
    ///     Finds the glideslope and DME associated with a localizer
    /// </summary>
    /// <param name="localizer">
    ///     Localizer whose associated navaid are being searched.
    /// </param>
    /// <param name="navData">
    ///     NavData containing the available glideslope and DME records.
    /// </param>
    /// <returns>
    ///     A match containing the original localizer and its associated glideslope and DME records
    ///     when available.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Throw when either argument is null
    /// </exception>
    LocalizerNavAidMatch Match(Localizer localizer, NavData navData);
}