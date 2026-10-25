using XPlaneLocalizerFixer.Repositories;
using XPlaneLocalizerFixer.Services;

namespace XPlaneLocalizerFixer.Test;

public class LocalizerRunwayMatcherTest
{
    private readonly IAirportRepository _airportRepository;
    private readonly ILocalizerRunwayMatcher _matcher;
    private readonly INavDataRepository _navDataRepository;

    public LocalizerRunwayMatcherTest(
        IAirportRepository airportRepository,
        INavDataRepository navDataRepository,
        ILocalizerRunwayMatcher matcher
    )
    {
        _airportRepository = airportRepository;
        _navDataRepository = navDataRepository;
        _matcher = matcher;
    }

    public void Run(string airportIcao, string runwayIdentifier)
    {
        var airports = _airportRepository.GetAirports();
        var navData = _navDataRepository.GetNavData();
        var localizer = navData.Localizers
            .FirstOrDefault(loc =>
                loc.AirportIcao == airportIcao &&
                loc.RunwayIdentifier == runwayIdentifier);

        if (localizer is null)
        {
            Console.WriteLine($"No match for runway {runwayIdentifier} in {airportIcao.ToUpperInvariant()}");
            return;
        }

        var match = _matcher.Match(localizer, airports);

        if (match is not null)
        {
            Console.WriteLine($"Localizer: {match.Localizer.Identifier}");
            Console.WriteLine($"Airport: {match.Airport.IcaoCode}");
            Console.WriteLine($"Runway: {match.Runway.Identifier}");

            Console.WriteLine($"Start: {match.Runway.StartPosition}");
            Console.WriteLine($"End: {match.Runway.EndPosition}");
        }
    }
}