namespace XPlaneLocalizerFixer.Services;

public sealed class XPlaneInstallationValidator : IXPlaneInstallationValidator
{
    public bool IsValidInstallation(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (!Directory.Exists(path))
            return false;


        // C:\X-Plane 12\Custom Scenery\
        var customSceneryPath = Path.Combine(path, "Custom Scenery");

        // C:\X-Plane 12\Global Scenery\Global Airports\Earth nav data\apt.dat
        var aptDatPath = Path.Combine(
            path, "Global Scenery", "Global Airports", "Earth nav data", "apt.dat");

        // C:\X-Plane 12\Custom Data\earth_nav.dat
        var customNavPath = Path.Combine(
            path, "Custom Data", "earth_nav.dat");

        // C:X-Plane 12\Resources\default data\earth_nav.dat
        var defaultNavPath = Path.Combine(
            path, "Resources", "default data", "earth_nav.dat");

        return
            Directory.Exists(customSceneryPath) &&
            File.Exists(aptDatPath) &&
            (File.Exists(customNavPath) || File.Exists(defaultNavPath));
    }
}