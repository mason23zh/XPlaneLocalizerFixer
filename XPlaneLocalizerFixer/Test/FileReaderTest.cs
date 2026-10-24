using XPlaneLocalizerFixer.Repositories.IO;

namespace XPlaneLocalizerFixer.Test;

public sealed class FileReaderTest
{
    private readonly IFileReader _fileReader;

    public FileReaderTest(IFileReader fileReader)
    {
       _fileReader = fileReader;     
    }

    public void Run(string fileName)
    {
        int lineCount = 0;

        foreach (var line in _fileReader.ReadLines(fileName))
        {
            lineCount++;

            if (lineCount <= 5)
            {
                Console.WriteLine(line);
            }
        }

        Console.WriteLine($"Total lines: {lineCount:N0}");
    } 
}