using System.Text;

namespace XPlaneLocalizerFixer.Repositories.IO;

public class FileWriter : IFileWriter
{
    public void WriteLines(string outputPath, IEnumerable<string> lines)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(lines);

        using var stream = new FileStream(
            outputPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        using var writer = new StreamWriter(
            stream, new UTF8Encoding(false))
        {
            NewLine = "\n"
        };

        foreach (var line in lines)
            writer.WriteLine(line);
    }
}