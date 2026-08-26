namespace TestRunner.WebApp.Features.AppLogging.Services;

/// <summary>
/// Configuration of the application log.
/// </summary>
public sealed class AppLoggingOptions
{
    /// <summary>
    /// Gets the maximum number of entries kept in memory. Older entries are dropped first.
    /// </summary>
    public int Capacity { get; init; } = 2000;

    /// <summary>
    /// Gets the directory the log files are written to.
    /// </summary>
    public string LogsDirectoryPath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TestRunner.WebApp",
        "logs");

    /// <summary>
    /// Gets the number of log files kept on disk; the oldest ones are deleted on startup.
    /// </summary>
    public int RetainedFileCount { get; init; } = 5;
}