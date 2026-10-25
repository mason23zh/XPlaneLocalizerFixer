using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories.IO;

namespace XPlaneLocalizerFixer.Test;

public class EarthNavDatParserTest
{
    private readonly IFileReader _fileReader;
    private readonly IEarthNavDatParser _parser;

    public EarthNavDatParserTest(
        IFileReader fileReader,
        IEarthNavDatParser parser)
    {
        _fileReader = fileReader;
        _parser = parser;
    }

    public void Run(string icao)
    {
        var lines = _fileReader.ReadLines("earth_nav.dat");

        var navData = _parser.Parse(lines);

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