namespace TestRunner.WebApp.Application.Logging;

using Microsoft.Extensions.Logging;

/// <summary>
/// One log entry written to the log file.
/// </summary>
/// <param name="Timestamp">Time of the entry.</param>
/// <param name="Level">Severity written to the log file.</param>
/// <param name="Category">Logger category of the entry.</param>
/// <param name="Message">Formatted message; later lines become detail.</param>
/// <param name="Exception">Exception written with the entry, if any.</param>
internal sealed record FileLogEntry(
    DateTimeOffset Timestamp,
    LogLevel Level,
    string Category,
    string Message,
    Exception? Exception);