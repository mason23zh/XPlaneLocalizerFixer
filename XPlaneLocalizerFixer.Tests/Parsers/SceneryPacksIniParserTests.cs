using Moq;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories.IO;

namespace XPlaneLocalizerFixer.Tests.Parsers;

public sealed class SceneryPacksIniParserTests
{
    [Fact]
    public void Parse_WhenEnabledSceneryPacksExist_ReturnsPackagesInOrder()
    {
        // Arrange
        const string filePath = "scenery_packs.ini";

        string[] lines =
        [
            "I",
            "1000 Version",
            "SCENERY",
            "",
            "SCENERY_PACK Custom Scenery/Aerosoft - LFMN Nice Cote d Azur X/",
            "SCENERY_PACK Custom Scenery/VHHX - Kai_Tak/",
            "SCENERY_PACK *GLOBAL_AIRPORTS*"
        ];

        var fileReader = new Mock<IFileReader>();

        fileReader
            .Setup(reader => reader.ReadLines(filePath))
            .Returns(lines);

        var parser = new SceneryPacksIniParser(fileReader.Object);

        // Act
        var result = parser.Parse(filePath);


        // Assert
        Assert.Equal(3, result.Count);

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal(
                    "Custom Scenery/Aerosoft - LFMN Nice Cote d Azur X/",
                    first.Path);
                Assert.True(first.IsEnabled);
                Assert.Equal(0, first.Priority);
            },
            second =>
            {
                Assert.Equal(
                    "Custom Scenery/VHHX - Kai_Tak/",
                    second.Path);
                Assert.True(second.IsEnabled);
                Assert.Equal(1, second.Priority);
            },
            third =>
            {
                Assert.Equal("*GLOBAL_AIRPORTS*", third.Path);
                Assert.True(third.IsEnabled);
                Assert.Equal(2, third.Priority);
            });
    }

    [Fact]
    public void Parse_WhenDisabledSceneryPacksExist_PreservesEnabledStatusAndPriority()
    {
        // Arrange
        const string filePath = "scenery_packs.ini";

        string[] lines =
        [
            "I",
            "1000 Version",
            "SCENERY",
            "",
            "SCENERY_PACK Custom Scenery/VHHX - Kai_Tak/",
            "SCENERY_PACK_DISABLED Custom Scenery/Aerosoft - LFMN Nice Cote d Azur X/",
            "SCENERY_PACK Custom Scenery/LPMA - 1 - Madeira airport/",
            "SCENERY_PACK *GLOBAL_AIRPORTS*"
        ];

        var fileReader = new Mock<IFileReader>();

        fileReader
            .Setup(reader => reader.ReadLines(filePath))
            .Returns(lines);

        var parser = new SceneryPacksIniParser(fileReader.Object);

        // Act
        var result = parser.Parse(filePath);

        // Assert
        Assert.Equal(4, result.Count);

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal("Custom Scenery/VHHX - Kai_Tak/", first.Path);
                Assert.True(first.IsEnabled);
                Assert.Equal(0, first.Priority);
            },
            second =>
            {
                Assert.Equal(
                    "Custom Scenery/Aerosoft - LFMN Nice Cote d Azur X/",
                    second.Path);
                Assert.False(second.IsEnabled);
                Assert.Equal(1, second.Priority);
            },
            third =>
            {
                Assert.Equal(
                    "Custom Scenery/LPMA - 1 - Madeira airport/",
                    third.Path);
                Assert.True(third.IsEnabled);
                Assert.Equal(2, third.Priority);
            },
            fourth =>
            {
                Assert.Equal("*GLOBAL_AIRPORTS*", fourth.Path);
                Assert.True(fourth.IsEnabled);
                Assert.Equal(3, fourth.Priority);
            });
    }

    [Fact]
    public void Parse_WhenNonSceneryEntriesExist_IgnoresThem()
    {
        // Arrange
        const string filePath = "scenery_packs.ini";

        string[] lines =
        [
            "I",
            "1000 Version",
            "SCENERY",
            "",
            "   ",
            "# This is a comment",
            "UNKNOWN_ENTRY SomeRandomCrap",
            "SCENERY_PACK Custom Scenery/VHHX - Kai_Tak/",
            "",
            "SCENERY_PACK ",
            "SCENERY_PACK *GLOBAL_AIRPORTS*"
        ];

        var fileReader = new Mock<IFileReader>();

        fileReader
            .Setup(reader => reader.ReadLines(filePath))
            .Returns(lines);

        var parser = new SceneryPacksIniParser(fileReader.Object);

        // Act
        var result = parser.Parse(filePath);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal("Custom Scenery/VHHX - Kai_Tak/", first.Path);
                Assert.True(first.IsEnabled);
                Assert.Equal(0, first.Priority);
            },
            second =>
            {
                Assert.Equal("*GLOBAL_AIRPORTS*", second.Path);
                Assert.True(second.IsEnabled);
                Assert.Equal(1, second.Priority);
            });
    }

    [Fact]
    public void Parse_WhenEntriesContainExtraWhitespace_TrimsPathsCorrectly()
    {
        // Arrange
        const string filePath = "scenery_packs.ini";

        string[] lines =
        [
            "   SCENERY_PACK Custom Scenery/VHHX - Kai_Tak/   ",
            "\tSCENERY_PACK Custom Scenery/Aerosoft - LFMN Nice Cote d Azur X/\t",
            "  SCENERY_PACK_DISABLED Custom Scenery/LPMA - 1 - Madeira airport/  ",
            "  SCENERY_PACK *GLOBAL_AIRPORTS*  "
        ];

        var fileReader = new Mock<IFileReader>();

        fileReader
            .Setup(reader => reader.ReadLines(filePath))
            .Returns(lines);

        var parser = new SceneryPacksIniParser(fileReader.Object);

        // Act
        var result = parser.Parse(filePath);

        // Assert
        Assert.Equal(4, result.Count);

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal("Custom Scenery/VHHX - Kai_Tak/", first.Path);
                Assert.True(first.IsEnabled);
                Assert.Equal(0, first.Priority);
            },
            second =>
            {
                Assert.Equal(
                    "Custom Scenery/Aerosoft - LFMN Nice Cote d Azur X/",
                    second.Path);
                Assert.True(second.IsEnabled);
                Assert.Equal(1, second.Priority);
            },
            third =>
            {
                Assert.Equal(
                    "Custom Scenery/LPMA - 1 - Madeira airport/",
                    third.Path);
                Assert.False(third.IsEnabled);
                Assert.Equal(2, third.Priority);
            },
            fourth =>
            {
                Assert.Equal("*GLOBAL_AIRPORTS*", fourth.Path);
                Assert.True(fourth.IsEnabled);
                Assert.Equal(3, fourth.Priority);
            });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Parse_WhenPathIsNullOrWhitespace_ThrowsArgumentException(string? path)
    {
        // Arrange
        var fileReader = new Mock<IFileReader>();
        var parser = new SceneryPacksIniParser(fileReader.Object);

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => parser.Parse(path!));

        // The parser should reject invalid paths before accessing the file reader.
        fileReader.Verify(
            reader => reader.ReadLines(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void Parse_WhenFileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        var filePath = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString(),
            "scenery_packs.ini");

        var fileReader = new FileReader();
        var parser = new SceneryPacksIniParser(fileReader);

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => parser.Parse(filePath));

        Assert.Equal(filePath, exception.FileName);
    }
}