using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Repositories;

/// <summary>
///     Defines operations for retrieving navaid data from Xplane earth_nav.dat file
/// </summary>
public interface INavDataRepository
{
    /// <summary>
    ///     Load navaid records from earth_nav.dat
    /// </summary>
    /// <returns>
    ///     A NavData instance containing the parsed localizer, glideslope and DME records.
    /// </returns>
    NavData GetNavData();
}