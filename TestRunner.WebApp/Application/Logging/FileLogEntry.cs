namespace TestRunner.WebApp.Application.Logging;

using Microsoft.Extensions.Logging;

/// <summary>
/// A single record on its way to the log file.
/// </summary>
/// <param name="Timestamp">Moment the record was created, which decides the file it lands in.</param>
/// <param name="Level">Level the record was logged with.</param>
/// <param name="Category">Logger category the record came from.</param>
/// <param name="Message">Formatted message; lines after the first are treated as detail.</param>
/// <param name="Exception">Optional exception attached to the record.</param>
internal sealed record FileLogEntry(
    DateTimeOffset Timestamp,
    LogLevel Level,
    string Category,
    string Message,
    Exception? Exception);