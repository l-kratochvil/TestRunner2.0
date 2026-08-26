namespace TestRunner.WebApp.Features.AppLogging.Models;

using TestRunner.WebApp.Shared.Logging;

/// <summary>
/// A single entry of the application log.
/// </summary>
/// <param name="Timestamp">Moment the entry was created.</param>
/// <param name="Severity">Severity of the entry.</param>
/// <param name="Source">Channel the entry belongs to, see <see cref="LogSources"/>.</param>
/// <param name="Message">Single-line, human readable message.</param>
/// <param name="Detail">Optional multi-line detail, such as a stack trace.</param>
public sealed record LogEntry(
    DateTimeOffset Timestamp,
    LogSeverity Severity,
    string Source,
    string Message,
    string? Detail = null);