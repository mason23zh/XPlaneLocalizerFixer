using System.Globalization;
using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Repositories.IO;
using XPlaneLocalizerFixer.Repositories.IO.Interfaces;
using XPlaneLocalizerFixer.Services.Interfaces;

namespace XPlaneLocalizerFixer.Services;

public class CorrectedNavDatWriter : ICorrectedNavDatWriter
{
    private const string Header =
        "I\n" +
        "1200 Version - data cycle 2407, build 20240630, metadata NavXP1200.";

    private readonly IFileWriter _fileWriter;

    public CorrectedNavDatWriter(IFileWriter fileWriter)
    {
        _fileWriter = fileWriter;
    }

    public void Write(string outputPath,
        IReadOnlyList<(LocalizerCorrectionResult Correction, LocalizerNavAidMatch NavAids)> corrections)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(corrections);

        // validate the entire collection before creating the output file
        foreach (var (correction, navAids) in corrections)
            if (!correction.IsCorrected ||
                correction.CorrectedLatitude is null ||
                correction.CorrectedLongitude is null ||
                correction.CorrectedEncodedCourse is null ||
                !navAids.IsEligibleForCorrection ||
                !ReferenceEquals(correction.Localizer, navAids.Localizer))
                throw new InvalidOperationException(
                    "The correction collection contains an invalid ILS correction.");

        _fileWriter.WriteLines(outputPath, GenerateLines(corrections));
    }

    private static IEnumerable<string> GenerateLines(
        IReadOnlyList<(LocalizerCorrectionResult Correction, LocalizerNavAidMatch NavAids)> corrections)
    {
        yield return Header;
        yield return string.Empty;

        foreach (var (correction, navAids) in corrections)
        {
            yield return FormatLocalizer(correction);
            yield return navAids.GlideSlope!.RawLine;

            if (navAids.Dme is not null)
                yield return navAids.Dme.RawLine;
        }

        yield return string.Empty;
        yield return "99";
    }

    private static string FormatLocalizer(LocalizerCorrectionResult correction)
    {
        var localizer = correction.Localizer;

        var latitude = correction.CorrectedLatitude!.Value;
        var longitude = correction.CorrectedLongitude!.Value;
        var encodedCourse = correction.CorrectedEncodedCourse!.Value;

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{localizer.RowCode,2} " +
            $"{latitude,13:F9} " +
            $"{longitude,14:F9} " +
            $"{localizer.ElevationFeet,8} " +
            $"{localizer.Frequency,8} " +
            $"{localizer.RangeNm,5} " +
            $"{encodedCourse,11:F3}  " +
            $"{localizer.Identifier} " +
            $"{localizer.AirportIcao} " +
            $"{localizer.RegionCode} " +
            $"{localizer.RunwayIdentifier} " +
            $"{localizer.Name}");
    }
}