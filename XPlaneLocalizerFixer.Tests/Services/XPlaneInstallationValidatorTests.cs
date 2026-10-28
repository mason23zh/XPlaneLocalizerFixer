using XPlaneLocalizerFixer.Services;

namespace XPlaneLocalizerFixer.Tests.Services;

public class XPlaneInstallationValidatorTests
{
    [Fact]
    public void IsValidInstallation_WithCustomNavData_ReturnsTrue()
    {
        // Arrange
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        try
        {
            Directory.CreateDirectory(
                Path.Combine(root, "Custom Scenery"));

            var aptDirectory = Path.Combine(
                root, "Global Scenery", "Global Airports", "Earth nav data");

            Directory.CreateDirectory(aptDirectory);
            File.WriteAllText(Path.Combine(aptDirectory, "apt.dat"), "");

            var customDataDirectory = Path.Combine(root, "Custom Data");
            Directory.CreateDirectory(customDataDirectory);
            File.WriteAllText(
                Path.Combine(customDataDirectory, "earth_nav.dat"), "");

            var validator = new XPlaneInstallationValidator();

            // Act
            var result = validator.IsValidInstallation(root);

            // Assert
            Assert.True(result);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Fact]
    public void IsValidInstallation_WithDefaultNavData_ReturnsTrue()
    {
        // Arrange
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        try
        {
            Directory.CreateDirectory(
                Path.Combine(root, "Custom Scenery"));

            var aptDirectory = Path.Combine(
                root, "Global Scenery", "Global Airports", "Earth nav data");

            Directory.CreateDirectory(aptDirectory);
            File.WriteAllText(Path.Combine(aptDirectory, "apt.dat"), "");

            var defaultDataDirectory = Path.Combine(
                root, "Resources", "default data");

            Directory.CreateDirectory(defaultDataDirectory);
            File.WriteAllText(
                Path.Combine(defaultDataDirectory, "earth_nav.dat"), "");

            var validator = new XPlaneInstallationValidator();

            // Act
            var result = validator.IsValidInstallation(root);

            // Assert
            Assert.True(result);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Fact]
    public void IsValidInstallation_WhenNavDataMissing_ReturnsFalse()
    {
        // Arrange
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        try
        {
            Directory.CreateDirectory(
                Path.Combine(root, "Custom Scenery"));

            var aptDirectory = Path.Combine(
                root, "Global Scenery", "Global Airports", "Earth nav data");

            Directory.CreateDirectory(aptDirectory);
            File.WriteAllText(Path.Combine(aptDirectory, "apt.dat"), "");

            var validator = new XPlaneInstallationValidator();

            // Act
            var result = validator.IsValidInstallation(root);

            // Assert
            Assert.False(result);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Fact]
    public void IsValidInstallation_WhenAptDatMissing_ReturnsFalse()
    {
        // Arrange
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        try
        {
            Directory.CreateDirectory(
                Path.Combine(root, "Custom Scenery"));

            var customDataDirectory = Path.Combine(root, "Custom Data");

            Directory.CreateDirectory(customDataDirectory);
            File.WriteAllText(
                Path.Combine(customDataDirectory, "earth_nav.dat"), "");

            var validator = new XPlaneInstallationValidator();

            // Act
            var result = validator.IsValidInstallation(root);

            // Assert
            Assert.False(result);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Fact]
    public void IsValidInstallation_WhenCustomSceneryMissing_ReturnsFalse()
    {
        // Arrange
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        try
        {
            var aptDirectory = Path.Combine(
                root, "Global Scenery", "Global Airports", "Earth nav data");

            Directory.CreateDirectory(aptDirectory);
            File.WriteAllText(Path.Combine(aptDirectory, "apt.dat"), "");

            var customDataDirectory = Path.Combine(root, "Custom Data");

            Directory.CreateDirectory(customDataDirectory);
            File.WriteAllText(
                Path.Combine(customDataDirectory, "earth_nav.dat"), "");

            var validator = new XPlaneInstallationValidator();

            // Act
            var result = validator.IsValidInstallation(root);

            // Assert
            Assert.False(result);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Fact]
    public void IsValidInstallation_WhenDirectoryDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var path = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString());

        var validator = new XPlaneInstallationValidator();

        // Act
        var result = validator.IsValidInstallation(path);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void IsValidInstallation_WhenPathIsNullOrWhitespace_ReturnsFalse(string? path)
    {
        // Arrange
        var validator = new XPlaneInstallationValidator();

        // Act
        var result = validator.IsValidInstallation(path!);

        // Assert
        Assert.False(result);
    }
}