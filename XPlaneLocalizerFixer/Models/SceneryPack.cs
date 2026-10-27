namespace XPlaneLocalizerFixer.Models;

/// <summary>
///     Represents a scenery_pack.ini config entry in the XPlane.
/// </summary>
/// <param name="Path">
///     The scenery pack path.
/// </param>
/// <param name="IsEnabled">
///     Whether the scenery pack is enabled.
/// </param>
/// <param name="Priority">
///     Package loading priority.
/// </param>
public record SceneryPack(
    string Path,
    bool IsEnabled,
    int Priority);