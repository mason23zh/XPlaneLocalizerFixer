using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories;
using XPlaneLocalizerFixer.Repositories.IO;

namespace XPlaneLocalizerFixer.Tests.Repositories;

public sealed class CustomSceneryIntegrationTests
{
    [Fact]
    public void GetExcludedAirportIdentifiers_WithEnabledAirportsAndLibrary_ReturnsOnlyAirports()
    {
        // Arrange
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        try
        {
            var customScenery = Path.Combine(root, "Custom Scenery");
            Directory.CreateDirectory(customScenery);

            var sceneryIniPath = Path.Combine(customScenery, "scenery_packs.ini");

            File.WriteAllLines(sceneryIniPath,
            [
                "I",
                "1000 Version",
                "SCENERY",
                "",
                "SCENERY_PACK Custom Scenery/VHHX - Kai_Tak/",
                "SCENERY_PACK Custom Scenery/VMMC - Macau/",
                "SCENERY_PACK *GLOBAL_AIRPORTS*",
                "SCENERY_PACK Custom Scenery/openSAM_Library/"
            ]);

            var vhhxDirectory = Path.Combine(
                customScenery, "VHHX - Kai_Tak", "Earth nav data");

            Directory.CreateDirectory(vhhxDirectory);
            File.WriteAllLines(Path.Combine(vhhxDirectory, "apt.dat"),
            [
                "I",
                "1200 Version",
                "1 100 0 0 VHHX Kai Tak Airport",
                "100 45.00 1 0 0.25 1 3 0 ...",
                "99"
            ]);

            var vmmcDirectory = Path.Combine(
                customScenery, "VMMC - Macau", "Earth nav data");

            Directory.CreateDirectory(vmmcDirectory);
            File.WriteAllLines(Path.Combine(vmmcDirectory, "apt.dat"),
            [
                "I",
                "1200 Version",
                "1 100 0 0 VMMC Macau International",
                "100 45.00 1 0 0.25 1 3 0 ...",
                "99"
            ]);

            // The library has no airport database.
            Directory.CreateDirectory(
                Path.Combine(customScenery, "openSAM_Library"));

            var fileReader = new FileReader();
            var parser = new SceneryPacksIniParser(fileReader);
            var repository = new CustomAirportRepository(fileReader);

            // Act
            var sceneryPacks = parser.Parse(sceneryIniPath);

            var excludedAirports = repository.GetExcludedAirportIdentifiers(
                root,
                sceneryPacks);

            // Assert
            Assert.Equal(4, sceneryPacks.Count);

            Assert.Equal(2, excludedAirports.Count);
            Assert.Contains("VHHX", excludedAirports);
            Assert.Contains("VMMC", excludedAirports);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }
}