namespace TestRunner.WebApp.Shared.Logging;

/// <summary>
/// Severity of a single application log entry, ordered from the most to the least verbose.
/// </summary>
/// <remarks>
/// There is no debug severity: the application log is what the user reads, so developer detail is
/// logged through <see cref="Microsoft.Extensions.Logging.ILogger"/> instead and never reaches the
/// panel.
/// </remarks>
public enum LogSeverity
{
    /// <summary>
    /// Normal progress of the application, including successful outcomes.
    /// </summary>
    Info,

    /// <summary>
    /// Something unexpected happened, but the application keeps working.
    /// </summary>
    Warning,

    /// <summary>
    /// An operation failed.
    /// </summary>
    Error,
}