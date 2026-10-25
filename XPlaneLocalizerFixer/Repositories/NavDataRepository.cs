using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories.IO;

namespace XPlaneLocalizerFixer.Repositories;

/// <summary>
///     Access to navaid records stored in Xplane earth_nav.dat file.
/// </summary>
public sealed class NavDataRepository : INavDataRepository
{
    private const string FileName = "earth_nav.dat";

    private readonly IFileReader _fileReader;
    private readonly IEarthNavDatParser _parser;

    public NavDataRepository(
        IFileReader fileReader,
        IEarthNavDatParser parser)
    {
        _fileReader = fileReader;
        _parser = parser;
    }

    public NavData GetNavData()
    {
        var liens = _fileReader.ReadLines(FileName);

        return _parser.Parse(liens);
    }
}