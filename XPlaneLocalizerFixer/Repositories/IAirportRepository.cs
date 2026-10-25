using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Repositories;

/// <summary>
/// Defines operations for retrieving airport and ruwnay data
/// from XPlane's airport dataset.
/// </summary>
public interface IAirportRepository
{
   /// <summary>
   /// Loads airports and their runways from apt.dat
   /// </summary>
   /// <returns>
   /// A readonly colelction of parsed airport records.
   /// </returns>
   IReadOnlyList<Airport> GetAirports();
}