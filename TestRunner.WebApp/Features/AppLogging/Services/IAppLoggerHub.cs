namespace TestRunner.WebApp.Features.AppLogging.Services;

using TestRunner.WebApp.Features.AppLogging.Models;

/// <summary>
/// The application-wide log buffer. Shared by every browser connected to this server.
/// </summary>
public interface IAppLoggerHub
{
    /// <summary>
    /// Raised on the thread of the caller that appended the entry.
    /// </summary>
    event Action<LogEntry>? EntryAppended;

    /// <summary>
    /// Appends the entry to the buffer and hands it to every sink.
    /// </summary>
    /// <param name="entry">Entry to append.</param>
    void Append(LogEntry entry);

    /// <summary>
    /// Records that something feeding the log has failed.
    /// </summary>
    /// <remarks>
    /// The failure reaches the buffer only and is deliberately kept away from the sinks: the one
    /// that failed would either fail again or, when it is the far end of the logging pipeline the
    /// log itself feeds, loop straight back into it.
    /// </remarks>
    /// <param name="message">Description of the failure, shown to the user.</param>
    void ReportFailure(string message);

    /// <summary>
    /// Takes a snapshot of the buffered entries, oldest first.
    /// </summary>
    /// <returns>The buffered entries.</returns>
    IReadOnlyList<LogEntry> GetEntries();
}