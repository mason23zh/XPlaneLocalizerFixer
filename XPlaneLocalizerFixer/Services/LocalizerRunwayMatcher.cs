using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Services;

/// <summary>
///     Mathces Xplane localizers to runway in the apt.dat
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
                    canditate.Runway.Identifier,
                    localizer.RunwayIdentifier,
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
}