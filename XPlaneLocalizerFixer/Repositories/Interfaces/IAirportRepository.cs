using XPlaneLocalizerFixer.Models;

namespace XPlaneLocalizerFixer.Repositories.Interfaces;

/// <summary>
///     Defines operations for retrieving airport and runway data
///     from XPlane's airport dataset.
/// </summary>
public interface IAirportRepository
{
   /// <summary>
   ///     Loads airports and their runways from apt.dat
   /// </summary>
   /// <param name="aptDatPath">
   ///     The full path to apt.dat
   /// </param>
   /// <returns>
   ///     A readonly collection of parsed airport records.
   /// </returns>
   IReadOnlyList<Airport> GetAirports(string aptDatPath);
}