using Microsoft.Extensions.Options;
using XPlaneLocalizerFixer.Configuration;
using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories.IO;

namespace XPlaneLocalizerFixer.Repositories;

/// <summary>
///     Retrieve airport and runway info from XPlane's apt.dat file
/// </summary>
/// <remarks>
///     Use file reader to stream the source file and parser to build airport and runway model
/// </remarks>
public sealed class AirportRepository : IAirportRepository
{
    private readonly IFileReader _fileReader;
    private readonly FilePathOptions _options;
    private readonly IAptDatParser _parser;

    public AirportRepository(
        IFileReader fileReader,
        IAptDatParser parser,
        IOptions<FilePathOptions> options)
    {
        _fileReader = fileReader;
        _parser = parser;
        _options = options.Value;
    }

    public IReadOnlyList<Airport> GetAirports()
    {
        var filePath = Path.Combine(
            AppContext.BaseDirectory,
            _options.InputDirectory,
            _options.AptDatFileName);

        var lines = _fileReader.ReadLines(filePath);

        return _parser.Parse(lines);
    }
}