namespace XPlaneLocalizerFixer.Repositories.IO;

public interface IFileReader
{
   /// <summary>
   ///     Reads text file one line at a time
   /// </summary>
   /// <param name="filePath">
   ///     Absolute or relative path to the file
   /// </param>
   /// <returns>Sequence of text lines</returns>
   /// <exception cref="ArgumentException">
   ///     Throw when invalid filename
   /// </exception>
   /// <exception cref="FileNotFoundException">
   ///     Throw when file not found
   /// </exception>
   IEnumerable<string> ReadLines(string filePath);
}