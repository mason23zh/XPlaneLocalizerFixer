using XPlaneLocalizerFixer.Repositories;

namespace XPlaneLocalizerFixer.Test;

public class EarthNavDatParserTest
{
    private readonly INavDataRepository _navDataRepository;

    public EarthNavDatParserTest(INavDataRepository navDataRepository)
    {
        _navDataRepository = navDataRepository;
    }


    public void Run(string icao)
    {
        var navData = _navDataRepository.GetNavData();

        Console.WriteLine($"Localizers: {navData.Localizers.Count}");
        Console.WriteLine($"Glideslopes: {navData.GlideSlopes.Count}");
        Console.WriteLine($"DMEs: {navData.Dmes.Count}");

        Console.WriteLine($"navaid for airport: {icao}");

        foreach (var localizer in navData.Localizers
                     .Where(x => x.AirportIcao == icao))
            Console.WriteLine(localizer.RawLine);

        foreach (var gs in navData.GlideSlopes
                     .Where(x => x.AirportIcao == icao))
            Console.WriteLine(gs.RawLine);

        foreach (var dme in navData.Dmes
                     .Where(x => x.AirportIcao == icao))
            Console.WriteLine(dme.RawLine);
    }
}