namespace XPlaneLocalizerFixer.Configuration;

/// <summary>
///     Config the input and output directories and file names
///     used by the application.
/// </summary>
public sealed class FilePathOptions
{
    public const string SectionName = "FilePath";

    /// <summary>
    ///     Directory containing the input nav data files.
    /// </summary>
    /// <remarks>
    ///     earth_nav.dat and apt.dat
    /// </remarks>
    public string InputDirectory { get; set; } = "input";

    /// <summary>
    ///     Directory containing the generated nav data file.
    /// </summary>
    public string OutputDirectory { get; set; } = "output";

    /// <summary>
    ///     Airport data file name
    /// </summary>
    /// <remarks>
    ///     Default location:
    ///     X-Plane 12\Global Scenery\Global Airports\Earth nav data\apt.dat
    /// </remarks>
    public string AptDatFileName { get; set; } = "apt.dat";

    /// <summary>
    ///     Nav data file name
    /// </summary>
    /// <remarks>
    ///     Default location:
    ///     X-Plane 12\Custom Data\earth_nav.dat
    /// </remarks>
    public string EarthNavDatFileName { get; set; } = "earth_nav.dat";

    /// <summary>
    ///     File name of the generated nav data file.
    /// </summary>
    /// <remarks>
    ///     XP-NAV1200 spec data with corrected localizer coordinates and course
    /// </remarks>
    public string OutputFileName { get; set; } = "user_nav_generated.dat";
}