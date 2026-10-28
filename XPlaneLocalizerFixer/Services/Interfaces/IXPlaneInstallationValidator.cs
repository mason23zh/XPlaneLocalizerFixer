namespace XPlaneLocalizerFixer.Services.Interfaces;

/// <summary>
///     Validates X-Plane installation directories.
/// </summary>
public interface IXPlaneInstallationValidator
{
    /// <summary>
    ///     Determines whether the specified path contains a valid
    ///     XPlane installation.
    /// </summary>
    /// <param name="path">
    ///     The Xplane install directory
    /// </param>
    /// <returns>True if directory contains a valid xplane installation.</returns>
    bool IsValidInstallation(string path);
}