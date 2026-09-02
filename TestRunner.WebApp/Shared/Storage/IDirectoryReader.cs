namespace TestRunner.WebApp.Shared.Storage;

/// <summary>
/// Reads what a folder on the machine the application runs on contains.
/// </summary>
/// <remarks>
/// A seam over the file system, so that what is made of the folder's contents can be exercised
/// without a folder.
/// </remarks>
public interface IDirectoryReader
{
    /// <summary>
    /// Reads the names of the folders directly inside the given folder.
    /// </summary>
    /// <param name="path">Full path of the folder to read.</param>
    /// <returns>The names, in no particular order.</returns>
    /// <exception cref="IOException">The folder could not be read.</exception>
    IReadOnlyList<string> ReadSubFolderNames(string path);
}
