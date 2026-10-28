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
            writer.Write(outputPath, excludedAirports);

            // Assert
            var lines = File.ReadAllLines(outputPath);

            Assert.Equal(
                ["LFMN", "LPMA", "VHHX", "VMMC"],
                lines.Skip(4).Take(4));

            Assert.Contains("# Total excluded airports: 4", lines);
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }
}