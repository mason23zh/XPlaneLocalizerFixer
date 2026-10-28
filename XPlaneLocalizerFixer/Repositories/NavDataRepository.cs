using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Parsers.Interfaces;
using XPlaneLocalizerFixer.Repositories.Interfaces;
using XPlaneLocalizerFixer.Repositories.IO;
using XPlaneLocalizerFixer.Repositories.IO.Interfaces;

namespace XPlaneLocalizerFixer.Repositories;

/// <summary>
///     Access to navaid records stored in Xplane earth_nav.dat file.
/// </summary>
public sealed class NavDataRepository : INavDataRepository
{
    private readonly IFileReader _fileReader;
    private readonly IEarthNavDatParser _parser;

    public NavDataRepository(
        IFileReader fileReader,
        IEarthNavDatParser parser
    )
    {
        _fileReader = fileReader;
        _parser = parser;
    }

    public NavData GetNavData(string earthNavDatPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(earthNavDatPath);

        var liens = _fileReader.ReadLines(earthNavDatPath);

        return _parser.Parse(liens);
    }
}