namespace XPlaneLocalizerFixer.Repositories.IO.Interfaces;

public interface IFileWriter
{
   /// <summary>
   ///     Write a sequence of text lines to a new file
   /// </summary>
   /// <param name="outputPath">
   ///     Destination path for the output file
   /// </param>
   /// <param name="lines">
   ///     LInes to write the file
   /// </param>
   /// <exception cref="ArgumentException">
   ///     Throw when the output path is empty or whitespace
   /// </exception>
   /// <exception cref="ArgumentNullException">
   ///     Throw when line is null
   /// </exception>
   /// <exception cref="IOException">
   ///     Throw when I/O error occurs.
   /// </exception>
   void WriteLines(string outputPath, IEnumerable<string> lines);
}