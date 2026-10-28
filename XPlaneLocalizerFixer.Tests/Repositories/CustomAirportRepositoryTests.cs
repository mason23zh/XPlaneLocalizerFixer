using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Repositories;
using XPlaneLocalizerFixer.Repositories.IO;

namespace XPlaneLocalizerFixer.Tests.Repositories;

public sealed class CustomAirportRepositoryTests
{
    [Fact]
    public void GetExcludedAirportIdentifiers_WhenCustomAirportHasRunway_ExcludesAirport()
    {
        // Arrange
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        try
        {
            var aptDirectory = Path.Combine(
                root,
                "Custom Scenery",
                "VHHX - Kai_Tak",
                "Earth nav data");

            Directory.CreateDirectory(aptDirectory);

            var aptDatPath = Path.Combine(aptDirectory, "apt.dat");

            File.WriteAllLines(aptDatPath,
            [
                "I",
                "1200 Version",
                "",
                "1     19 0 0 VHHX Wonderful Kai Tak",
                "100 60.00 26 523 0.00 1 3 0  13  22.32516342  114.19261369 556 176 2 1 0 2 31  22.30405346",
                "99"
            ]);

            SceneryPack[] sceneryPacks =
            [
                new("Custom Scenery/VHHX - Kai_Tak/", true, 0),
                new("*GLOBAL_AIRPORTS*", true, 1)
            ];

            var repository = new CustomAirportRepository(new FileReader());

            // Act
            var result = repository.GetExcludedAirportIdentifiers(
                root,
                sceneryPacks);

            // Assert
            Assert.Single(result);
            Assert.Contains("VHHX", result);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GetExcludedAirportIdentifiers_WhenSceneryIsDisabled_DoesNotExcludeAirport()
    {
        // Arrange
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        try
        {
            var aptDirectory = Path.Combine(
                root,
                "Custom Scenery",
                "VHHX - Kai_Tak",
                "Earth nav data");

            Directory.CreateDirectory(aptDirectory);

            var aptDatPath = Path.Combine(aptDirectory, "apt.dat");

            File.WriteAllLines(aptDatPath,
            [
                "I",
                "1200 Version",
                "",
                "1     19 0 0 VHHX Wonderful Kai Tak",
                "100 60.00 26 523 0.00 1 3 0  13  22.32516342  114.19261369 556 176 2 1 0 2 31  22.30405346",
                "99"
            ]);

            SceneryPack[] sceneryPacks =
            [
                new("Custom Scenery/VHHX - Kai_Tak/", false, 0),
                new("*GLOBAL_AIRPORTS*", true, 1)
            ];

            var repository = new CustomAirportRepository(new FileReader());

            // Act
            var result = repository.GetExcludedAirportIdentifiers(
                root,
                sceneryPacks);

            // Assert
            Assert.Empty(result);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GetExcludedAirportIdentifiers_WhenGlobalAirportsEntryIsMissing_StillExcludesAirport()
    {
        // Arrange
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        try
        {
            var aptDirectory = Path.Combine(
                root,
                "Custom Scenery",
                "VHHX - Kai_Tak",
                "Earth nav data");

            Directory.CreateDirectory(aptDirectory);

            var aptDatPath = Path.Combine(aptDirectory, "apt.dat");

            File.WriteAllLines(aptDatPath,
            [
                "I",
                "1200 Version",
                "",
                "1     19 0 0 VHHX Wonderful Kai Tak",
                "100 60.00 26 523 0.00 1 3 0  13  22.32516342  114.19261369 556 176 2 1 0 2 31  22.30405346",
                "99"
            ]);

            // No *GLOBAL_AIRPORTS* entry is present.
            SceneryPack[] sceneryPacks =
            [
                new("Custom Scenery/VHHX - Kai_Tak/", true, 0)
            ];

            var repository = new CustomAirportRepository(new FileReader());

            // Act
            var result = repository.GetExcludedAirportIdentifiers(
                root,
                sceneryPacks);

            // Assert
            Assert.Single(result);
            Assert.Contains("VHHX", result);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GetExcludedAirportIdentifiers_WhenAptDatIsMissing_DoesNotExcludeAirport()
    {
        // Arrange
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        try
        {
            // Simulate an installed scenery package without apt.dat.
            Directory.CreateDirectory(
                Path.Combine(root, "Custom Scenery", "openSAM_Library"));

            SceneryPack[] sceneryPacks =
            [
                new("Custom Scenery/openSAM_Library/", true, 0)
            ];

            var repository = new CustomAirportRepository(new FileReader());

            // Act
            var result = repository.GetExcludedAirportIdentifiers(
                root,
                sceneryPacks);

            // Assert
            Assert.Empty(result);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    // not sure if this would ever happen, just to be safe. 
    [Fact]
    public void GetExcludedAirportIdentifiers_WhenAptDatContainsMultipleAirports_ExcludesOnlyAirportsWithRunways()
    {
        // Arrange
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        try
        {
            var aptDirectory = Path.Combine(
                root,
                "Custom Scenery",
                "Custom Airports",
                "Earth nav data");

            Directory.CreateDirectory(aptDirectory);

            var aptDatPath = Path.Combine(aptDirectory, "apt.dat");

            File.WriteAllLines(aptDatPath,
            [
                "I",
                "1200 Version",
                "",
                "1 100 0 0 CYWG WINNIPEG INTL",
                "100 45.00 1 0 0.25 1 3 0 ...",
                "",
                "1 100 0 0 KDEN DENVER INTL",
                "100 45.00 1 0 0.25 1 3 0 ...",
                "",
                "1 100 0 0 TEST Airport Without Runway",
                "1302 city Test City",
                "99"
            ]);

            SceneryPack[] sceneryPacks =
            [
                new("Custom Scenery/Custom Airports/", true, 0)
            ];

            var repository = new CustomAirportRepository(new FileReader());

            // Act
            var result = repository.GetExcludedAirportIdentifiers(
                root,
                sceneryPacks);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("CYWG", result);
            Assert.Contains("KDEN", result);
            Assert.DoesNotContain("TEST", result);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GetExcludedAirportIdentifiers_WhenMultipleSceneryPacksDefineSameAirport_ExcludesAirportOnce()
    {
        // Arrange
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        try
        {
            var firstAptDirectory = Path.Combine(
                root,
                "Custom Scenery",
                "VHHX Airport",
                "Earth nav data");

            var secondAptDirectory = Path.Combine(
                root,
                "Custom Scenery",
                "VHHX Airport Update",
                "Earth nav data");

            Directory.CreateDirectory(firstAptDirectory);
            Directory.CreateDirectory(secondAptDirectory);

            File.WriteAllLines(
                Path.Combine(firstAptDirectory, "apt.dat"),
                [
                    "I",
                    "1200 Version",
                    "",
                    "1     19 0 0 VHHX Wonderful Kai Tak",
                    "100 60.00 26 523 0.00 1 3 0  13  22.32516342  114.19261369 556 176 2 1 0 2 31  22.30405346",
                    "99"
                ]);

            File.WriteAllLines(
                Path.Combine(secondAptDirectory, "apt.dat"),
                [
                    "I",
                    "1200 Version",
                    "",
                    "1     19 0 0 VHHX Wonderful Kai Tak",
                    "100 60.00 26 523 0.00 1 3 0  13  22.32516342  114.19261369 556 176 2 1 0 2 31  22.30405346",
                    "99"
                ]);

            SceneryPack[] sceneryPacks =
            [
                new("Custom Scenery/VHHX Airport/", true, 0),
                new("Custom Scenery/VHHX Airport Update/", true, 1)
            ];

            var repository = new CustomAirportRepository(new FileReader());

            // Act
            var result = repository.GetExcludedAirportIdentifiers(
                root,
                sceneryPacks);

            // Assert
            Assert.Single(result);
            Assert.Contains("VHHX", result);
            Assert.Contains("vhhx", result);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GetExcludedAirportIdentifiers_WhenAirportHasNoRunway_DoesNotExcludeAirport()
    {
        // Arrange
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        try
        {
            var aptDirectory = Path.Combine(
                root,
                "Custom Scenery",
                "Test Airport",
                "Earth nav data");

            Directory.CreateDirectory(aptDirectory);

            File.WriteAllLines(
                Path.Combine(aptDirectory, "apt.dat"),
                [
                    "I",
                    "1200 Version",
                    "",
                    "1 100 0 0 TEST Airport Without Runway",
                    "1302 city Test City",
                    "99"
                ]);

            SceneryPack[] sceneryPacks =
            [
                new("Custom Scenery/Test Airport/", true, 0)
            ];

            var repository = new CustomAirportRepository(new FileReader());

            // Act
            var result = repository.GetExcludedAirportIdentifiers(
                root,
                sceneryPacks);

            // Assert
            Assert.Empty(result);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    // should never happen, but who knows
    [Fact]
    public void GetExcludedAirportIdentifiers_WhenAirportHeaderIsInvalid_ThrowsInvalidDataException()
    {
        // Arrange
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        try
        {
            var aptDirectory = Path.Combine(
                root,
                "Custom Scenery",
                "Invalid Airport",
                "Earth nav data");

            Directory.CreateDirectory(aptDirectory);

            File.WriteAllLines(
                Path.Combine(aptDirectory, "apt.dat"),
                [
                    "I",
                    "1200 Version",
                    "1 100 0",
                    "100 45.00 1 0 0.25 1 3 0 ...",
                    "99"
                ]);

            SceneryPack[] sceneryPacks =
            [
                new("Custom Scenery/Invalid Airport/", true, 0)
            ];

            var repository = new CustomAirportRepository(new FileReader());

            // Act & Assert
            Assert.Throws<InvalidDataException>(() => repository.GetExcludedAirportIdentifiers(root, sceneryPacks));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GetExcludedAirportIdentifiers_WhenSceneryPathEscapesRoot_ThrowsInvalidDataException()
    {
        // Arrange
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        SceneryPack[] sceneryPacks =
        [
            new("Custom Scenery/../../../OtherFolder/", true, 0)
        ];

        var repository = new CustomAirportRepository(new FileReader());

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => repository.GetExcludedAirportIdentifiers(root, sceneryPacks));
    }

    [Fact]
    public void GetExcludedAirportIdentifiers_WhenSceneryPathIsAbsolute_ThrowsInvalidDataException()
    {
        // Arrange
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        var absoluteSceneryPath = Path.Combine(
            Path.GetTempPath(),
            "External Scenery");

        SceneryPack[] sceneryPacks =
        [
            new(absoluteSceneryPath, true, 0)
        ];

        var repository = new CustomAirportRepository(new FileReader());

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => repository.GetExcludedAirportIdentifiers(root, sceneryPacks));
    }
}