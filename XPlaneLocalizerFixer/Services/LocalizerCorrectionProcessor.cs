using XPlaneLocalizerFixer.Models;
using XPlaneLocalizerFixer.Repositories;

namespace XPlaneLocalizerFixer.Services;

public sealed class LocalizerCorrectionProcessor : ILocalizerCorrectionProcessor
{
    private readonly IAirportRepository _airportRepository;
    private readonly ILocalizerCorrector _corrector;
    private readonly ILocalizerNavAidMatcher _navAidMatcher;
    private readonly INavDataRepository _navDataRepository;
    private readonly ILocalizerRunwayMatcher _runwayMatcher;

    public LocalizerCorrectionProcessor(
        IAirportRepository airportRepository,
        INavDataRepository navDataRepository,
        ILocalizerNavAidMatcher navAidMatcher,
        ILocalizerRunwayMatcher runwayMatcher,
        ILocalizerCorrector corrector
    )
    {
        _airportRepository = airportRepository;
        _navDataRepository = navDataRepository;
        _navAidMatcher = navAidMatcher;
        _runwayMatcher = runwayMatcher;
        _corrector = corrector;
    }

    public IReadOnlyList<(LocalizerCorrectionResult Correction, LocalizerNavAidMatch NavAids)> Process()
    {
        var airports = _airportRepository.GetAirports();
        var navData = _navDataRepository.GetNavData();

        var corrections = new List<(
            LocalizerCorrectionResult Correction,
            LocalizerNavAidMatch NavAids
            )>();

        foreach (var loc in navData.Localizers)
        {
            // only ILS paired localizers are considered
            if (loc.RowCode != 4)
                continue;

            LocalizerNavAidMatch navAidMatch;

            try
            {
                navAidMatch = _navAidMatcher.Match(loc, navData);
            }
            catch (InvalidOperationException)
            {
                continue;
            }

            if (!navAidMatch.IsEligibleForCorrection)
                continue;

            var runwayMatch = _runwayMatcher.Match(loc, airports);

            if (runwayMatch is null)
                continue;

            var result = _corrector.Correct(runwayMatch);

            if (!result.IsCorrected)
                continue;

            corrections.Add((result, navAidMatch));
        }

        return corrections;
    }
}