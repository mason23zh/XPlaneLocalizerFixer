using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories.IO;

namespace XPlaneLocalizerFixer.Repositories;

/// <summary>
///  Retrive airport and runway info from XPlane's apt.dat file
/// </summary>
/// <remarks>
/// Use file reader to stream the soruce file and parser to build airport and runway model</remarks>
/// 
public sealed class AirportRepository:IAirportRepository
{
    private const string FileName = "apt.dat";
    
    private readonly IFileReader _fileReader;
    private readonly AptDatParser _parser;

    public AirportRepository(
        IFileReader fileReader,
        AptDatParser parser)
    {
       _fileReader = fileReader;
       _parser = parser;
    }

    public IReadOnlyList<Airport> GetAirports()
    {
        IEnumerable<string> lines = _fileReader.ReadLines(FileName);
        
        return _parser.Parse(lines);
    }
}