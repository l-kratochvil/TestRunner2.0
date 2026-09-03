namespace TestRunner.WebApp.Application.Logging;

/// <summary>
/// Options that configure the file logger, bound from <c>Logging:File</c>.
/// </summary>
public sealed class FileLoggerOptions
{
    /// <summary>
    /// Gets the directory holding the log files.
    /// </summary>
    public string Path { get; init; } = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TestRunner.WebApp",
        "logs");

    /// <summary>
    /// Gets how many log files are kept on disk.
    /// </summary>
    public int RetainedFileCount { get; init; } = 5;
}