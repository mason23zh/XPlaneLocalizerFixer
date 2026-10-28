namespace XPlaneLocalizerFixer.Configuration;

/// <summary>
///     Config the input and output directories and file names
///     used by the application.
/// </summary>
public sealed class FilePathOptions
{
    public const string SectionName = "FilePaths";

    /// <summary>
    ///     Directory containing the generated nav data file.
    /// </summary>
    public string OutputDirectory { get; set; } = "output";

    /// <summary>
    ///     File name of the generated nav data file.
    /// </summary>
    /// <remarks>
    ///     XP-NAV1200 spec data with corrected localizer coordinates and course
    /// </remarks>
    public string OutputFileName { get; set; } = "user_nav_generated.dat";

    /// <summary>
    ///     File that contains a list of airports that been excluded from the localizer corrections.
    /// </summary>
    public string ExcludedAirportsFileName { get; set; } = "excluded_airports.txt";
}