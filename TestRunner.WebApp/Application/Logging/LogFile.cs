namespace TestRunner.WebApp.Application.Logging;

using System.Globalization;

/// <summary>
/// Naming and retention of the log files: one file per day, named after that day, the newest
/// files are kept.
/// </summary>
public static class LogFile
{
    /// <summary>
    /// Extension of every file owned by the log.
    /// </summary>
    public const string Extension = ".log";

    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Builds the path of the log file the given moment belongs to.
    /// </summary>
    /// <param name="directoryPath">Directory holding the log files.</param>
    /// <param name="timestamp">Moment the entry was created.</param>
    /// <returns>Path of the log file.</returns>
    public static string GetPath(string directoryPath, DateTimeOffset timestamp)
    {
        string fileName = timestamp.ToString(DateFormat, CultureInfo.InvariantCulture) + Extension;

        return System.IO.Path.Combine(directoryPath, fileName);
    }

    /// <summary>
    /// Enumerates the files owned by the log, newest first.
    /// </summary>
    /// <remarks>
    /// The whole file name is the date, so a file is recognized by parsing that date rather than
    /// by a wildcard: DOS wildcards match more names than they appear to, and a mistake here
    /// deletes files that do not belong to the log.
    /// </remarks>
    /// <param name="directoryPath">Directory holding the log files.</param>
    /// <returns>Paths of the log files, newest first.</returns>
    public static IEnumerable<string> Enumerate(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return [];
        }

        // The date is the whole file name, so ordinal ordering by name is chronological.
        return Directory
            .EnumerateFiles(directoryPath)
            .Where(IsLogFile)
            .OrderByDescending(Path.GetFileName, StringComparer.Ordinal);
    }

    /// <summary>
    /// Deletes all but the newest log files.
    /// </summary>
    /// <remarks>
    /// Files are counted, not days: days on which the application never ran leave no file behind
    /// and must not consume a slot.
    /// </remarks>
    /// <param name="directoryPath">Directory holding the log files.</param>
    /// <param name="retainedFileCount">Number of files to keep.</param>
    public static void ApplyRetention(string directoryPath, int retainedFileCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(retainedFileCount);

        // Materialized before deleting, so that the enumeration is not invalidated underneath.
        List<string> obsoleteFilePaths = [..Enumerate(directoryPath).Skip(retainedFileCount)];

        foreach (string filePath in obsoleteFilePaths)
        {
            File.Delete(filePath);
        }
    }

    private static bool IsLogFile(string filePath)
    {
        if (!Path.GetExtension(filePath).Equals(Extension, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return DateOnly.TryParseExact(
            Path.GetFileNameWithoutExtension(filePath),
            DateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);
    }
}