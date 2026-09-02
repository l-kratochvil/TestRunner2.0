namespace TestRunner.WebApp.Shared.Storage;

using System.IO;
using System.Linq;

/// <inheritdoc/>
public sealed class DirectoryReader : IDirectoryReader
{
    /// <inheritdoc/>
    public IReadOnlyList<string> ReadSubFolderNames(string path)
        => [..Directory.GetDirectories(path).Select(Path.GetFileName).OfType<string>()];
}
