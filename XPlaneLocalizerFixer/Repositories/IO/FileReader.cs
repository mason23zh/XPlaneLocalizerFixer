using XPlaneLocalizerFixer.Repositories.IO.Interfaces;

namespace XPlaneLocalizerFixer.Repositories.IO;

/// <summary>
///     Stream access to text files that located in the application's directory
/// </summary>
public sealed class FileReader : IFileReader
{
    public IEnumerable<string> ReadLines(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException(
                $"Required file '{filePath}' not found.", filePath);

        return File.ReadLines(filePath);
    }
}