using System.Text;
using XPlaneLocalizerFixer.Repositories.IO;

namespace XPlaneLocalizerFixer.Tests.FileAccess;

public sealed class FileWriterTests : IDisposable
{
    private readonly string _testDirectory = Path.Combine(
        Path.GetTempPath(),
        $"XPlaneLocalizerFixerTests_{Guid.NewGuid():N}");

    private readonly FileWriter _writer = new();

    public FileWriterTests()
    {
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    ///     Removes temporary files created by the tests.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }

    [Fact]
    public void WriteLines_WhenValid_WritesLinesToFile()
    {
        // Arrange
        var outputPath = Path.Combine(_testDirectory, "test.dat");

        string[] lines =
        [
            "I",
            "1200 Version - test",
            "",
            "99"
        ];

        // Act
        _writer.WriteLines(outputPath, lines);

        // Assert
        Assert.True(File.Exists(outputPath));
        Assert.Equal(lines, File.ReadAllLines(outputPath));
    }

    [Fact]
    public void WriteLines_WhenFileAlreadyExists_OverwritesExistingFile()
    {
        // Arrange
        var outputPath = Path.Combine(_testDirectory, "existing.dat");

        File.WriteAllText(outputPath, "Old data");

        // Act
        _writer.WriteLines(outputPath, ["New data", "99"]);

        // Assert
        Assert.Equal(
            ["New data", "99"],
            File.ReadAllLines(outputPath));
    }

    [Fact]
    public void WriteLines_WhenOutputPathIsEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _writer.WriteLines("", ["Test"]));
    }

    [Fact]
    public void WriteLines_WhenLinesIsNull_ThrowsArgumentNullException()
    {
        var outputPath = Path.Combine(_testDirectory, "test.dat");

        Assert.Throws<ArgumentNullException>(() =>
            _writer.WriteLines(outputPath, null!));
    }

    [Fact]
    public void WriteLines_WhenValid_UsesUtf8WithoutBomAndUnixLineEndings()
    {
        // Arrange
        var outputPath = Path.Combine(_testDirectory, "encoding.dat");

        // Act
        _writer.WriteLines(outputPath, ["I", "99"]);

        // Assert
        var bytes = File.ReadAllBytes(outputPath);

        Assert.Equal("I\n99\n", Encoding.UTF8.GetString(bytes));
        Assert.False(bytes.AsSpan().StartsWith(Encoding.UTF8.GetPreamble()));
    }
}