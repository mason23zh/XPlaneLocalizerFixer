using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Services;

/// <summary>
///     Calculates localizer alignment corrections using associated runway geometry.
/// </summary>
public interface ILocalizerCorrector
{
   /// <summary>
   ///     Attempts to align a localizer with its associated runway centerline
   /// </summary>
   /// <param name="match">
   ///     The matched localizer, airport and runway
   /// </param>
   /// <returns>
   ///     A result contain the corrected position and course, or why correction was skipped.
   /// </returns>
   LocalizerCorrectionResult Correct(LocalizerRunwayMatch match);
}