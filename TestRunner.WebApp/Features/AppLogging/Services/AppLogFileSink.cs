namespace TestRunner.WebApp.Features.AppLogging.Services;

using System.Text;
using System.Threading;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using TestRunner.WebApp.Features.AppLogging.Models;
using TestRunner.WebApp.Shared.Logging;

/// <summary>
/// Mirrors every log entry, including <see cref="LogSeverity.Debug"/>, into a daily log file.
/// </summary>
/// <remarks>
/// Entries are handed over through a channel and written by a background loop, so that logging
/// never blocks the caller on disk I/O. Writing is best effort: a failure is reported once and
/// the application keeps running with the in-memory log only.
/// </remarks>
public sealed class AppLogFileSink : IAppLogSink, IHostedService, IDisposable
{
    private readonly AppLoggingOptions options;
    private readonly Channel<LogEntry> channel = Channel.CreateUnbounded<LogEntry>(
        new UnboundedChannelOptions { SingleReader = true });

    private readonly CancellationTokenSource stopping = new();
    private Task? writerTask;
    private bool failureReported;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppLogFileSink"/> class.
    /// </summary>
    /// <param name="options">Configuration of the application log.</param>
    public AppLogFileSink(AppLoggingOptions options)
    {
        this.options = options;
    }

    /// <inheritdoc/>
    public event Action<string>? Failed;

    /// <summary>
    /// Gets the path of the log file the entries are currently written to.
    /// </summary>
    public string CurrentFilePath => AppLogFiles.GetFilePath(this.options.LogsDirectoryPath, DateTimeOffset.Now);

    /// <inheritdoc/>
    public void Write(LogEntry entry)
    {
        this.channel.Writer.TryWrite(entry);
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.PrepareLogsDirectory();
        this.writerTask = Task.Run(() => this.RunAsync(this.stopping.Token), CancellationToken.None);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        this.channel.Writer.TryComplete();

        if (this.writerTask is null)
        {
            return;
        }

        try
        {
            await this.writerTask.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Shutdown is being forced; whatever is left in the channel is dropped.
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // The same instance is registered as the sink and as a hosted service, so the container
        // disposes it more than once.
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
        this.stopping.Cancel();
        this.stopping.Dispose();
    }

    private static bool IsFileSystemFailure(Exception exception)
    {
        return exception is IOException or UnauthorizedAccessException or NotSupportedException;
    }

    private void PrepareLogsDirectory()
    {
        try
        {
            Directory.CreateDirectory(this.options.LogsDirectoryPath);

            // Today's file must exist before retention runs, otherwise it would not count as one
            // of the retained files and an extra old file would survive.
            string currentFilePath = this.CurrentFilePath;
            if (!File.Exists(currentFilePath))
            {
                File.Create(currentFilePath).Dispose();
            }

            AppLogFiles.ApplyRetention(this.options.LogsDirectoryPath, this.options.RetainedFileCount);
        }
        catch (Exception exception) when (IsFileSystemFailure(exception))
        {
            this.ReportFailure(exception);
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        List<LogEntry> batch = [];

        try
        {
            while (await this.channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                batch.Clear();

                while (this.channel.Reader.TryRead(out LogEntry? entry))
                {
                    batch.Add(entry);
                }

                this.WriteBatch(batch);
            }
        }
        catch (OperationCanceledException)
        {
            // The application is shutting down.
        }
    }

    private void WriteBatch(IReadOnlyCollection<LogEntry> batch)
    {
        if (batch.Count == 0)
        {
            return;
        }

        // A batch can span midnight, so entries are grouped by the file they belong to.
        foreach (IGrouping<string, LogEntry> group in batch.GroupBy(
            entry => AppLogFiles.GetFilePath(this.options.LogsDirectoryPath, entry.Timestamp),
            StringComparer.OrdinalIgnoreCase))
        {
            StringBuilder builder = new();

            foreach (LogEntry entry in group)
            {
                AppLogFileFormatter.AppendTo(builder, entry);
            }

            try
            {
                File.AppendAllText(group.Key, builder.ToString());
            }
            catch (Exception exception) when (IsFileSystemFailure(exception))
            {
                this.ReportFailure(exception);
            }
        }
    }

    private void ReportFailure(Exception exception)
    {
        if (this.failureReported)
        {
            return;
        }

        this.failureReported = true;
        this.Failed?.Invoke(
            $"Writing to the log file failed, the log is kept in memory only. {exception.Message}");
    }
}