namespace TestRunner.WebApp.Features.AppLogging.Services;

using System.Threading;
using TestRunner.WebApp.Features.AppLogging.Models;
using TestRunner.WebApp.Shared.Logging;

/// <summary>
/// In-memory ring buffer of log entries that also fans entries out to the registered sinks.
/// </summary>
public sealed class AppLogStore : IAppLogStore
{
    private readonly Lock gate = new();
    private readonly Queue<LogEntry> entries = new();
    private readonly int capacity;
    private readonly IReadOnlyList<IAppLogSink> sinks;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppLogStore"/> class.
    /// </summary>
    /// <param name="options">Configuration of the application log.</param>
    /// <param name="sinks">Destinations the entries are mirrored to.</param>
    public AppLogStore(AppLoggingOptions options, IEnumerable<IAppLogSink> sinks)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(options.Capacity, 1);

        this.capacity = options.Capacity;
        this.sinks = [.. sinks];

        foreach (IAppLogSink sink in this.sinks)
        {
            sink.Failed += this.OnSinkFailed;
        }
    }

    /// <inheritdoc/>
    public event Action<LogEntry>? EntryAppended;

    /// <inheritdoc/>
    public void Append(LogEntry entry)
    {
        this.AppendToBuffer(entry);

        foreach (IAppLogSink sink in this.sinks)
        {
            sink.Write(entry);
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<LogEntry> GetEntries()
    {
        lock (this.gate)
        {
            return [.. this.entries];
        }
    }

    private void AppendToBuffer(LogEntry entry)
    {
        lock (this.gate)
        {
            this.entries.Enqueue(entry);

            while (this.entries.Count > this.capacity)
            {
                this.entries.Dequeue();
            }
        }

        this.EntryAppended?.Invoke(entry);
    }

    private void OnSinkFailed(string message)
    {
        // Only the in-memory buffer is used here: routing the failure back through the sinks
        // would either loop or fail again.
        this.AppendToBuffer(new LogEntry(DateTimeOffset.Now, LogSeverity.Error, LogSources.App, message));
    }
}