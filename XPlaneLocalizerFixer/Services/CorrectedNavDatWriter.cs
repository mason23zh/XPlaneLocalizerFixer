using System.Globalization;
using System.Text;
using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Services;

public class CorrectedNavDatWriter : ICorrectedNavDatWriter
{
    private const string Header =
        "I\n" +
        "1200 Version - data cycle 2407, build 20240630, metadata NavXP1200.";

    public void Write(string outputPath,
        IReadOnlyList<(LocalizerCorrectionResult Correction, LocalizerNavAidMatch NavAids)> corrections)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(corrections);

        // validate the entire colleciton before creating the output file
        foreach (var (correction, navAids) in corrections)
            if (!correction.IsCorrected ||
                correction.CorrectedLatitude is null ||
                correction.CorrectedLongitude is null ||
                correction.CorrectedEncodedCourse is null ||
                !navAids.IsEligibleForCorrection ||
                !ReferenceEquals(correction.Localizer, navAids.Localizer))
                throw new InvalidOperationException(
                    "The correction collection contains an invalid ILS correction.");

        using var stream = new FileStream(
            outputPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None);

        using var writer = new StreamWriter(
            stream,
            new UTF8Encoding(false));

        writer.NewLine = "\n";

        writer.WriteLine(Header);

        foreach (var (correction, navAdis) in corrections)
        {
            writer.WriteLine(FormatLocalizer(correction));
            writer.WriteLine(navAdis.GlideSlope!.RawLine);

            if (navAdis.Dme is not null) writer.WriteLine(navAdis.Dme.RawLine);
        }

        writer.WriteLine("99");
    }

    private static string FormatLocalizer(LocalizerCorrectionResult correction)
    {
        var localizer = correction.Localizer;

        return string.Join(" ",
            localizer.RowCode.ToString(CultureInfo.InvariantCulture),
            correction.CorrectedLatitude!.Value.ToString("F9", CultureInfo.InvariantCulture),
            correction.CorrectedLongitude!.Value.ToString("F9", CultureInfo.InvariantCulture),
            localizer.ElevationFeet.ToString(CultureInfo.InvariantCulture),
            localizer.Frequency.ToString(CultureInfo.InvariantCulture),
            localizer.RangeNm.ToString(CultureInfo.InvariantCulture),
            correction.CorrectedEncodedCourse!.Value.ToString("F3", CultureInfo.InvariantCulture),
            localizer.Identifier,
            localizer.AirportIcao,
            localizer.RegionCode,
            localizer.RunwayIdentifier,
            localizer.Name);
    }
}