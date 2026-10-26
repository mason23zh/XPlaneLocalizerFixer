using Microsoft.Extensions.Options;
using XPlaneLocalizerFixer.Configuration;

namespace XPlaneLocalizerFixer.Repositories.IO;

/// <summary>
///     Stream access to text files that located in the application's directory
/// </summary>
public sealed class FileReader : IFileReader
{
    private readonly FilePathOptions _options;

    public FileReader(IOptions<FilePathOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    public IEnumerable<string> ReadLines(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (Path.GetFileName(fileName) != fileName ||
            fileName is "." or ".." ||
            fileName.Contains('/') ||
            fileName.Contains('\\') ||
            Path.IsPathRooted(fileName))
            throw new ArgumentException(
                "Only file names are allowed.", nameof(fileName));


        var filePath = Path.Combine(
            AppContext.BaseDirectory,
            _options.InputDirectory,
            fileName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException(
                $"Required file '{fileName}' not found.", filePath);

        return File.ReadLines(filePath);
    }
}