using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Parsers;
using XPlaneLocalizerFixer.Repositories.IO;

namespace XPlaneLocalizerFixer.Tests.FileAccess;

public sealed class ExcludedAirportReportWriterTests
{
    [Fact]
    public void Write_WhenAirportsExist_GeneratesSortedReport()
    {
        // Arrange
        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.txt");

        IReadOnlySet<string> excludedAirports = new HashSet<string>
        {
            "VHHX",
            "LFMN",
            "VMMC",
            "LPMA"
        };

        try
        {
            var writer = new ExcludedAirportReportWriter(new FileWriter());

            // Act
            writer.Write(outputPath, excludedAirports, []);

            // Assert
            var lines = File.ReadAllLines(outputPath);

            var customScenerySection = Array.IndexOf(
                lines,
                "# Custom scenery exclusions");

            Assert.True(customScenerySection >= 0);

            Assert.Equal(
                ["LFMN", "LPMA", "VHHX", "VMMC"],
                lines.Skip(customScenerySection + 3).Take(4));

            Assert.Contains("# Total custom scenery airports: 4", lines);
            Assert.Contains("# Total rejected localizers: 0", lines);
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }


    [Fact]
    public void Write_WhenCorrectionsAreRejected_IncludesFailureReasonsAndDisplacements()
    {
        // Arrange
        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.txt");

        var navData = new EarthNavDatParser().Parse(
        [
            "4 -22.830111111 -43.236972222 17 11050 18 53765.679 IJB SBGL SB 15 ILS-cat-I",
            "4 22.295272222 113.895083333 20 11090 18 91690.804 ITFL VHHH VH 25L ILS-cat-II"
        ]);

        var sbglLocalizer = navData.Localizers[0];
        var vhhhLocalizer = navData.Localizers[1];

        var rejections = new List<LocalizerCorrectionResult>
        {
            new()
            {
                Localizer = vhhhLocalizer,
                IsCorrected = false,
                FailureReason = LocalizerCorrectionFailureReason.LateralDeviationExceeded,
                LateralDisplacementMeters = 65.25,
                SkipReason = "Lateral displacement exceeds 50 metres."
            },
            new()
            {
                Localizer = sbglLocalizer,
                IsCorrected = false,
                FailureReason = LocalizerCorrectionFailureReason.HeadingDeviationExceeded,
                SkipReason = "Heading deviation exceeds 1 degree."
            }
        };

        try
        {
            var writer = new ExcludedAirportReportWriter(new FileWriter());

            // Act
            writer.Write(outputPath, new HashSet<string>(), rejections);

            // Assert
            var lines = File.ReadAllLines(outputPath);

            Assert.Contains("SBGL RWY 15 (IJB)", lines);
            Assert.Contains("VHHH RWY 25L (ITFL)", lines);

            Assert.Contains(
                "  Reason: Heading deviation exceeds 1 degree.",
                lines);

            Assert.Contains(
                "  Reason: Lateral displacement exceeds 50 metres.",
                lines);

            Assert.Contains("  Lateral displacement: 65.25 m", lines);

            Assert.Contains("# Total custom scenery airports: 0", lines);
            Assert.Contains("# Total rejected localizers: 2", lines);

            // Rejections should be sorted by airport ICAO.
            var sbglIndex = Array.IndexOf(lines, "SBGL RWY 15 (IJB)");
            var vhhhIndex = Array.IndexOf(lines, "VHHH RWY 25L (ITFL)");

            Assert.True(sbglIndex >= 0);
            Assert.True(vhhhIndex > sbglIndex);
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }
}