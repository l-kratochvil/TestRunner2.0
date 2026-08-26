namespace TestRunner.WebApp.Shared.Logging;

/// <summary>
/// Writes entries to the application log under a fixed source.
/// </summary>
public interface IAppLogger
{
    /// <summary>
    /// Gets the source every entry of this logger is written under.
    /// </summary>
    string Source { get; }

    /// <summary>
    /// Logs verbose detail, hidden in the log panel by default.
    /// </summary>
    /// <param name="message">Single-line message.</param>
    /// <param name="detail">Optional multi-line detail.</param>
    void Debug(string message, string? detail = null);

    /// <summary>
    /// Logs normal progress, including successful outcomes.
    /// </summary>
    /// <param name="message">Single-line message.</param>
    /// <param name="detail">Optional multi-line detail.</param>
    void Info(string message, string? detail = null);

    /// <summary>
    /// Logs something unexpected that does not stop the application.
    /// </summary>
    /// <param name="message">Single-line message.</param>
    /// <param name="detail">Optional multi-line detail.</param>
    void Warning(string message, string? detail = null);

    /// <summary>
    /// Logs a failed operation.
    /// </summary>
    /// <param name="message">Single-line message.</param>
    /// <param name="detail">Optional multi-line detail.</param>
    void Error(string message, string? detail = null);

    /// <summary>
    /// Logs with a severity known only at run time, for example when forwarding foreign output.
    /// </summary>
    /// <param name="severity">Severity of the entry.</param>
    /// <param name="message">Single-line message.</param>
    /// <param name="detail">Optional multi-line detail.</param>
    void Log(LogSeverity severity, string message, string? detail = null);
}