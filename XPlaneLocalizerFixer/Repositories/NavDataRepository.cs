using Microsoft.Extensions.Options;
using XPlaneLocalizerFixer.Configuration;
using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories.IO;

namespace XPlaneLocalizerFixer.Repositories;

/// <summary>
///     Access to navaid records stored in Xplane earth_nav.dat file.
/// </summary>
public sealed class NavDataRepository : INavDataRepository
{
    private readonly IFileReader _fileReader;
    private readonly FilePathOptions _options;
    private readonly IEarthNavDatParser _parser;

    public NavDataRepository(
        IFileReader fileReader,
        IEarthNavDatParser parser,
        IOptions<FilePathOptions> options
    )
    {
        _fileReader = fileReader;
        _parser = parser;
        _options = options.Value;
    }

    public NavData GetNavData()
    {
        var filePath = Path.Combine(
            AppContext.BaseDirectory,
            _options.InputDirectory,
            _options.EarthNavDatFileName);

        var liens = _fileReader.ReadLines(filePath);

        return _parser.Parse(liens);
    }
}