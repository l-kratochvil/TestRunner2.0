namespace TestRunner.WebApp.Shared.Logging;

/// <summary>
/// Severity of a single application log entry, ordered from the most to the least verbose.
/// </summary>
public enum LogSeverity
{
    /// <summary>
    /// Verbose detail (for example raw test runner output). Hidden in the panel by default.
    /// </summary>
    Debug,

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