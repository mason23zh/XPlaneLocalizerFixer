using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories.IO;

namespace XPlaneLocalizerFixer.Test;

public sealed class AptDatParserTest
{
   private readonly IFileReader _fileReader;
   private readonly AptDatParser _parser;

   public AptDatParserTest(
      IFileReader fileReader,
      AptDatParser parser)
   {
     _fileReader = fileReader;
     _parser = parser;
   }

   public void Run(string icao)
   {
      IEnumerable<string> lines = _fileReader.ReadLines("apt.dat");

      IReadOnlyList<Airport> airports= _parser.Parse(lines);

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
      {
         Console.WriteLine(
            $"{runway.Identifier}::" +
            $"{runway.StartPosition.Latitude}," +
            $"{runway.StartPosition.Longitude}--" +
            $"{runway.EndPosition.Latitude}," +
            $"{runway.EndPosition.Longitude}");
      }
   }
}