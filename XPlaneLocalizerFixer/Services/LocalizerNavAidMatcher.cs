using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Services.Interfaces;

namespace XPlaneLocalizerFixer.Services;

/// <summary>
///     Match an ILS localizer with its associated glideslope and DME records.
/// </summary>
public sealed class LocalizerNavAidMatcher : ILocalizerNavAidMatcher
{
    public LocalizerNavAidMatch Match(Localizer localizer, NavData navData)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        ArgumentNullException.ThrowIfNull(navData);

        //skip stand-alone localizer like LOC, LDA & SDF
        if (localizer.RowCode != 4)
            return new LocalizerNavAidMatch
            {
                Localizer = localizer
            };

        var glideSlope = navData.GlideSlopes
            .SingleOrDefault(gs =>
                gs.RowCode == 6
                && MatchesCommonFields(localizer, gs)
                && string.Equals(
                    localizer.RunwayIdentifier,
                    gs.RunwayIdentifier,
                    StringComparison.OrdinalIgnoreCase));

        if (glideSlope is null)
            return new LocalizerNavAidMatch
            {
                Localizer = localizer
            };

        var dme = navData.Dmes
            .SingleOrDefault(candidate =>
                candidate.RowCode == 12
                && MatchesCommonFields(localizer, candidate));

        return new LocalizerNavAidMatch
        {
            Localizer = localizer,
            GlideSlope = glideSlope,
            Dme = dme
        };
    }


    /// <summary>
    ///     Determines whether two navaids share the identifying fields required for association.
    /// </summary>
    private static bool MatchesCommonFields(NavAid localizer, NavAid candidate)
    {
        return localizer.Frequency == candidate.Frequency &&
               string.Equals(
                   localizer.Identifier,
                   candidate.Identifier,
                   StringComparison.OrdinalIgnoreCase) &&
               !string.IsNullOrWhiteSpace(localizer.AirportIcao) &&
               string.Equals(
                   localizer.AirportIcao,
                   candidate.AirportIcao,
                   StringComparison.OrdinalIgnoreCase) &&
               string.Equals(
                   localizer.RegionCode,
                   candidate.RegionCode,
                   StringComparison.OrdinalIgnoreCase);
    }
}