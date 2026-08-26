namespace TestRunner.WebApp.Shared.Logging;

/// <summary>
/// Creates loggers bound to a log source.
/// </summary>
public interface IAppLoggerFactory
{
    /// <summary>
    /// Creates a logger writing every entry under the given source.
    /// </summary>
    /// <param name="source">Source of the entries, see <see cref="LogSources"/>.</param>
    /// <returns>The logger.</returns>
    IAppLogger CreateLogger(string source);
}