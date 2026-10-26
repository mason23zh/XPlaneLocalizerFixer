using XPlaneLocalizerFixer.Repositories.IO;

namespace XPlaneLocalizerFixer.Tests.FileAccess;

public sealed class FileReaderTests : IDisposable
{
    private readonly FileReader _reader = new();
    private readonly string _testDirectory;

    /// <summary>
    ///     Creates an isolated temporary directory for each test.
    /// </summary>
    public FileReaderTests()
    {
        _testDirectory = Path.Combine(
            Path.GetTempPath(),
            $"XPlaneLocalizerFixerTests_{Guid.NewGuid():N}");

        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    ///     Removes temporary files created during the test.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }

    [Fact]
    public void ReadLines_WhenFileExists_ReturnsAllLines()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "apt.dat");

        string[] expectedLines =
        [
            "I",
            "1200 Version - test",
            "",
            "99"
        ];

        File.WriteAllLines(filePath, expectedLines);

        // Act
        var result = _reader.ReadLines(filePath).ToArray();

        // Assert
        Assert.Equal(expectedLines, result);
    }

    [Fact]
    public void ReadLines_WhenFileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "missing.dat");

        // Act
        var exception = Assert.Throws<FileNotFoundException>(() => _reader.ReadLines(filePath).ToArray());

        // Assert
        Assert.Equal(filePath, exception.FileName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void ReadLines_WhenPathIsEmptyOrWhitespace_ThrowsArgumentException(
        string filePath)
    {
        Assert.Throws<ArgumentException>(() => _reader.ReadLines(filePath).ToArray());
    }

    [Fact]
    public void ReadLines_WhenPathIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _reader.ReadLines(null!).ToArray());
    }

    [Fact]
    public void ReadLines_WhenAbsolutePathIsProvided_ReadsFileSuccessfully()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "earth_nav.dat");

        File.WriteAllText(filePath, "99\n");

        // Act
        var result = _reader.ReadLines(filePath).ToArray();

        // Assert
        Assert.Equal(["99"], result);
    }

    [Fact]
    public void ReadLines_WhenFileIsEmpty_ReturnsEmptySequence()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "empty.dat");

        File.WriteAllText(filePath, string.Empty);

        // Act
        var result = _reader.ReadLines(filePath).ToArray();

        // Assert
        Assert.Empty(result);
    }
}