using XPlaneLocalizerFixer.Repositories;

namespace XPlaneLocalizerFixer.Test;

public sealed class AptDatParserTest
{
    private readonly IAirportRepository _airportRepository;

    public AptDatParserTest(IAirportRepository airportRepository)
    {
        _airportRepository = airportRepository;
    }


    public void Run(string icao)
    {
        var airports = _airportRepository.GetAirports();

        Console.WriteLine($"Airports parsed: {airports.Count}");

        var airport = airports.FirstOrDefault(a =>
            string.Equals(
                a.IcaoCode ?? a.Identifier,
                icao,
                StringComparison.OrdinalIgnoreCase));

        if (airport is null)
        {
            Console.WriteLine($"Airport {icao} not found.");
            return;
        }

        Console.WriteLine($"Airport: {airport.Name}");
        Console.WriteLine($"ICAO: {airport.IcaoCode ?? "N/A"}");
        Console.WriteLine($"Runways found: {airport.Runways.Count}");

        foreach (var runway in airport.Runways)
            Console.WriteLine(
                $"{runway.Identifier}::" +
                $"{runway.StartPosition.Latitude}," +
                $"{runway.StartPosition.Longitude}--" +
                $"{runway.EndPosition.Latitude}," +
                $"{runway.EndPosition.Longitude}");
    }
}