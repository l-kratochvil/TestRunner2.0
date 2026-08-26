namespace TestRunner.WebApp.Features.AppLogging.Services;

using TestRunner.WebApp.Features.AppLogging.Models;

/// <summary>
/// A destination the application log is mirrored to, in addition to the in-memory buffer.
/// </summary>
/// <remarks>
/// A sink is best effort: <see cref="Write"/> must never throw and must never block the caller,
/// because it runs on the hot path of whoever is logging.
/// </remarks>
public interface IAppLogSink
{
    /// <summary>
    /// Raised when the sink itself fails, so that the failure can be surfaced in the log panel.
    /// </summary>
    event Action<string>? Failed;

    /// <summary>
    /// Hands the entry over to the sink.
    /// </summary>
    /// <param name="entry">Entry to write.</param>
    void Write(LogEntry entry);
}