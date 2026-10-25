namespace XPlaneLocalizerFixer.Repositories.IO;

/// <summary>
///     Stream access to text files that located in the application's directory
/// </summary>
public class FileReader : IFileReader
{
    public IEnumerable<string> ReadLines(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (Path.GetFileName(fileName) != fileName || fileName is "." or "..")
            throw new ArgumentException($"Only file names are allowed. {nameof(fileName)}");


        var filePath = Path.Combine(
            AppContext.BaseDirectory,
            fileName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException(
                $"Required file '{fileName}' not found.", filePath);

        return File.ReadLines(filePath);
    }
}