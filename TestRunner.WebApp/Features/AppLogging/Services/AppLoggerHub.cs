namespace TestRunner.WebApp.Features.AppLogging.Services;

using System.Threading;
using TestRunner.WebApp.Features.AppLogging.Models;
using TestRunner.WebApp.Shared.Logging;

/// <summary>
/// In-memory ring buffer of log entries that also fans entries out to the registered sinks.
/// </summary>
public sealed class AppLoggerHub : IAppLoggerHub
{
    /// <summary>
    /// Number of entries kept in memory unless another capacity is asked for.
    /// </summary>
    public const int DefaultCapacity = 2000;

    private readonly Lock gate = new();
    private readonly Queue<LogEntry> entries = new();
    private readonly int capacity;
    private readonly IReadOnlyList<IAppLoggerSink> sinks;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppLoggerHub"/> class.
    /// </summary>
    /// <param name="sinks">Destinations the entries are mirrored to.</param>
    /// <param name="capacity">Maximum number of entries kept in memory.</param>
    public AppLoggerHub(IEnumerable<IAppLoggerSink> sinks, int capacity = DefaultCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);

        this.capacity = capacity;
        this.sinks = [..sinks];

        foreach (IAppLoggerSink sink in this.sinks)
        {
            sink.Failed += this.ReportFailure;
        }
    }

    /// <inheritdoc/>
    public event Action<LogEntry>? EntryAppended;

    /// <inheritdoc/>
    public void Append(LogEntry entry)
    {
        this.AppendToBuffer(entry);

        foreach (IAppLoggerSink sink in this.sinks)
        {
            sink.Write(entry);
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<LogEntry> GetEntries()
    {
        lock (this.gate)
        {
            return [..this.entries];
        }
    }

    /// <inheritdoc/>
    public void ReportFailure(string message)
    {
        this.AppendToBuffer(new LogEntry(DateTimeOffset.Now, LogSeverity.Error, LogSources.App, message));
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
}