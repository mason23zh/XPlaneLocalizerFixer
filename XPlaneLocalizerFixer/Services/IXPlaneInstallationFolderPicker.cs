namespace XPlaneLocalizerFixer.Services;

/// <summary>
///     Provide a native folder selection dialog for selecting XPlane root folder
/// </summary>
public interface IXPlaneInstallationFolderPicker
{
    /// <summary>
    ///     Opens a folder selection dialog.
    /// </summary>
    /// <returns>
    ///     The selected directory path, null if the user cancel.
    /// </returns>
    string? SelectFolder();
}