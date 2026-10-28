using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Services.Interfaces;

namespace XPlaneLocalizerFixer.Services;

/// <summary>
///     Matches Xplane localizers to runway in the apt.dat
/// </summary>
public sealed class LocalizerRunwayMatcher : ILocalizerRunwayMatcher
{
    public LocalizerRunwayMatch? Match(Localizer localizer, IEnumerable<Airport> airports)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        ArgumentNullException.ThrowIfNull(airports);

        if (string.IsNullOrWhiteSpace(localizer.AirportIcao) ||
            string.IsNullOrWhiteSpace(localizer.RunwayIdentifier))
            return null;

        var matches = airports
            .Where(airport =>
                string.Equals(
                    airport.IcaoCode ?? airport.Identifier,
                    localizer.AirportIcao,
                    StringComparison.OrdinalIgnoreCase))
            .SelectMany(airport =>
                airport.Runways.Select(runway =>
                    new { Airport = airport, Runway = runway }))
            .Where(canditate =>
                string.Equals(
                    NormalizeRunwayIdentifier(canditate.Runway.Identifier),
                    NormalizeRunwayIdentifier(localizer.RunwayIdentifier),
                    StringComparison.OrdinalIgnoreCase))
            .Take(2)
            .ToList();

        if (matches.Count != 1) return null;

        var match = matches[0];

        return new LocalizerRunwayMatch(
            localizer,
            match.Airport,
            match.Runway);
    }

    /// <summary>
    ///     Trim out the leading '0' in earth_nav.dat during the matching
    /// </summary>
    /// <remarks>
    ///     For example, the runway '7' from apt.dat will be matched to runway '7' in earth_nav.dat
    ///     otherwise it might match to runway '07' in earth_nav.dat, cause mismatch.
    /// </remarks>
    private static string NormalizeRunwayIdentifier(string runwayIdentifier)
    {
        return runwayIdentifier
            .Trim()
            .TrimStart('0')
            .ToUpperInvariant();
    }
}