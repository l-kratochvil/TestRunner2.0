namespace TestRunner.WebApp.Application.Logging;

/// <summary>
/// Configuration of the log file, bound from the <c>Logging:File</c> configuration section.
/// </summary>
public sealed class FileLoggerOptions
{
    /// <summary>
    /// Gets the directory the log files are written to.
    /// </summary>
    public string Path { get; init; } = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TestRunner.WebApp",
        "logs");

    /// <summary>
    /// Gets the number of log files kept on disk; the oldest ones are deleted.
    /// </summary>
    public int RetainedFileCount { get; init; } = 5;
}