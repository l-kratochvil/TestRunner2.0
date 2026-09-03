namespace TestRunner.WebApp.Shared.Storage;

/// <summary>
/// Reads what directories on the test machine contain.
/// </summary>
public interface IDirectoryReader
{
    /// <summary>
    /// Reads the names of the subdirectories directly under <paramref name="path"/>.
    /// </summary>
    /// <param name="path">Full path of the directory to read.</param>
    /// <returns>The subdirectory names, in no particular order.</returns>
    /// <exception cref="IOException">Reading <paramref name="path"/> failed.</exception>
    IReadOnlyList<string> ReadSubFolderNames(string path);
}
