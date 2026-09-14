namespace TestRunner.WebApp.Application.Logging;

using TestRunner.WebApp.Application.Paths;

/// <summary>
/// Options that configure the file logger, bound from <c>Logging:File</c>.
/// </summary>
public sealed class FileLoggerOptions
{
    /// <summary>
    /// Gets the directory holding the log files, empty unless configuration names one.
    /// </summary>
    /// <remarks>
    /// Left empty, the log files follow the application data root, see
    /// <see cref="IAppPathsProvider"/>.
    /// </remarks>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Gets how many log files are kept on disk.
    /// </summary>
    public int RetainedFileCount { get; init; } = 5;
}