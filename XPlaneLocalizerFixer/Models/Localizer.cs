namespace XPlaneLocalizerFixer.Models;

/// <summary>
///     Represents ILS localizer record from the X-Plane custom data/earth_nav.data file
/// </summary>
/// <remarks>
///     This will represent both Row Code 4 and 5
/// </remarks>
public sealed class Localizer : NavAid
{
    /// <summary>
    ///     Runway that associated with this localizer
    /// </summary>
    public required string RunwayIdentifier { get; init; }

    /// <summary>
    ///     Navaid description
    /// </summary>
    /// <example>
    ///     ILS-cat-II
    /// </example>
    public required string Name { get; init; }

    /// <summary>
    ///     Original combined magnetic and true course value stored in the
    ///     custom data/earth_nav.dat
    /// </summary>
    /// <remarks>Localizer bearing in true degrees prefixed by integer magnetic front course times 360</remarks>
    /// <example>
    ///     Front course of 164 degrees magnetic on localizer
    ///     true bearing of 180.343 degrees becomes 59,040 + 180.343 = 59,220.343
    ///     fmod(59220.343,360)==180.343
    /// </example>
    public required double EncodedCourse { get; init; }
}