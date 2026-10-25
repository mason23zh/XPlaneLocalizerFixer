namespace XPlaneLocalizerFixer.Models;

/// <summary>
///     Represents a successfully matched localizer, airport and runway.
/// </summary>
public sealed record LocalizerRunwayMatch(
    Localizer Localizer,
    Airport Airport,
    Runway Runway
);