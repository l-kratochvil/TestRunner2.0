namespace TestRunner.WebApp.Features.AppLogging.Services;

using System.Globalization;

/// <summary>
/// Naming and retention of the log files. One file per day, the newest files are kept.
/// </summary>
public static class AppLogFiles
{
    /// <summary>
    /// Pattern matching every file owned by the application log.
    /// </summary>
    public const string FileSearchPattern = "app-*.log";

    private const string FileNamePrefix = "app-";
    private const string FileNameExtension = ".log";

    /// <summary>
    /// Builds the path of the log file the entry belongs to.
    /// </summary>
    /// <param name="logsDirectoryPath">Directory holding the log files.</param>
    /// <param name="timestamp">Timestamp of the entry.</param>
    /// <returns>Path of the log file.</returns>
    public static string GetFilePath(string logsDirectoryPath, DateTimeOffset timestamp)
    {
        string fileName = string.Concat(
            FileNamePrefix,
            timestamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            FileNameExtension);

        return Path.Combine(logsDirectoryPath, fileName);
    }

    /// <summary>
    /// Deletes all but the newest log files.
    /// </summary>
    /// <remarks>
    /// Files are counted, not days: days on which the application never ran leave no file behind
    /// and must not consume a slot.
    /// </remarks>
    /// <param name="logsDirectoryPath">Directory holding the log files.</param>
    /// <param name="retainedFileCount">Number of files to keep.</param>
    public static void ApplyRetention(string logsDirectoryPath, int retainedFileCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(retainedFileCount);

        if (!Directory.Exists(logsDirectoryPath))
        {
            return;
        }

        // The date is part of the file name, so ordinal ordering by name is chronological.
        IEnumerable<string> obsoleteFilePaths = Directory
            .EnumerateFiles(logsDirectoryPath, FileSearchPattern)
            .OrderByDescending(Path.GetFileName, StringComparer.Ordinal)
            .Skip(retainedFileCount);

        foreach (string filePath in obsoleteFilePaths)
        {
            File.Delete(filePath);
        }
    }
}