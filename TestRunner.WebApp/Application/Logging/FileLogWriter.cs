namespace TestRunner.WebApp.Application.Logging;

using System.Globalization;
using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

/// <summary>
/// Appends log records to the daily log file and keeps the retention of those files.
/// </summary>
/// <remarks>
/// Records are handed over through a channel and written by a background loop, so that logging
/// never blocks the caller on disk I/O. Writing is best effort: a failure is reported once and
/// the application keeps running without the file.
/// </remarks>
internal sealed class FileLogWriter : IDisposable
{
    private const string DetailIndent = "    ";
    private const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";

    private static readonly TimeSpan FlushTimeout = TimeSpan.FromSeconds(5);

    private readonly string directoryPath;
    private readonly int retainedFileCount;

    private readonly Channel<FileLogEntry> channel =
        Channel.CreateUnbounded<FileLogEntry>(new UnboundedChannelOptions { SingleReader = true });

    private readonly CancellationTokenSource stopping = new();
    private readonly Task writerTask;

    private string? currentFilePath;
    private bool failureReported;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileLogWriter"/> class.
    /// </summary>
    /// <param name="directoryPath">Directory the log files are written to.</param>
    /// <param name="retainedFileCount">Number of log files kept on disk.</param>
    public FileLogWriter(string directoryPath, int retainedFileCount)
    {
        this.directoryPath = directoryPath;
        this.retainedFileCount = retainedFileCount;
        this.writerTask = Task.Run(() => this.RunAsync(this.stopping.Token), CancellationToken.None);
    }

    /// <summary>
    /// Raised the first time writing fails, so that the failure can be surfaced to the user.
    /// </summary>
    public event Action<string>? Failed;

    /// <summary>
    /// Hands the record over to the background writer.
    /// </summary>
    /// <param name="record">Record to write.</param>
    public void Write(FileLogEntry record)
    {
        this.channel.Writer.TryWrite(record);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // The provider is disposed both by the logger factory and by the container, so this runs
        // more than once.
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
        this.channel.Writer.TryComplete();

        // Draining happens during shutdown, so it must not be able to hang the process on a
        // stalled disk: whatever is left after the timeout is dropped.
        this.writerTask.Wait(FlushTimeout);

        this.stopping.Cancel();
        this.stopping.Dispose();
    }

    private static string GetLevelName(LogLevel level)
    {
        // Padded to a fixed width so that the columns of the file line up.
        return level switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO ",
            LogLevel.Warning => "WARN ",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "CRIT ",
            _ => "NONE ",
        };
    }

    /// <summary>
    /// Appends the record as one line, followed by its indented detail lines, to the builder.
    /// </summary>
    /// <param name="builder">Builder to append to.</param>
    /// <param name="record">Record to format.</param>
    private static void AppendRecord(StringBuilder builder, FileLogEntry record)
    {
        string[] messageLines = record.Message.ReplaceLineEndings("\n").Split('\n');

        builder
            .Append(record.Timestamp.ToString(TimestampFormat, CultureInfo.InvariantCulture))
            .Append(" [")
            .Append(GetLevelName(record.Level))
            .Append("] [")
            .Append(record.Category)
            .Append("] ")
            .AppendLine(messageLines[0]);

        // Everything after the first line is a continuation of the same record, indented so that
        // it cannot be mistaken for a new one.
        AppendDetail(builder, messageLines.Skip(1));

        if (record.Exception is not null)
        {
            AppendDetail(builder, record.Exception.ToString().ReplaceLineEndings("\n").Split('\n'));
        }
    }

    private static void AppendDetail(StringBuilder builder, IEnumerable<string> lines)
    {
        foreach (string line in lines)
        {
            builder.Append(DetailIndent).AppendLine(line);
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        List<FileLogEntry> batch = [];

        try
        {
            while (await this.channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                batch.Clear();

                while (this.channel.Reader.TryRead(out FileLogEntry? record))
                {
                    batch.Add(record);
                }

                this.WriteBatch(batch);
            }
        }
        catch (OperationCanceledException)
        {
            // The application is shutting down.
        }
        catch (Exception exception)
        {
            // Last resort. Nothing will ever be written again once this loop is gone, and a log
            // that stops silently is indistinguishable from one that has nothing to say.
            this.ReportFailure(exception);
        }
    }

    private void WriteBatch(IReadOnlyCollection<FileLogEntry> batch)
    {
        if (batch.Count == 0)
        {
            return;
        }

        // A batch can span midnight, so entries are grouped by the file they belong to.
        foreach (IGrouping<string, FileLogEntry> group in batch.GroupBy(
            record => LogFile.GetPath(this.directoryPath, record.Timestamp),
            StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                this.RollOverTo(group.Key);

                StringBuilder builder = new();

                foreach (FileLogEntry record in group)
                {
                    AppendRecord(builder, record);
                }

                File.AppendAllText(group.Key, builder.ToString());
            }
            catch (Exception exception)
            {
                // Every failure is caught, not just the expected file system ones: this batch is
                // lost either way, but the loop has to survive to keep draining the channel.
                // Letting the exception out would stop the writer for good and grow the queue
                // without bound.
                this.ReportFailure(exception);
            }
        }
    }

    /// <summary>
    /// Makes the file current and applies retention, which happens on the first write and again
    /// whenever the day changes underneath a running application.
    /// </summary>
    /// <param name="filePath">File the entries are about to be written to.</param>
    private void RollOverTo(string filePath)
    {
        if (string.Equals(this.currentFilePath, filePath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Directory.CreateDirectory(this.directoryPath);

        // The new file must exist before retention runs, otherwise it would not count as one
        // of the retained files and an extra old file would survive.
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Dispose();
        }

        LogFile.ApplyRetention(this.directoryPath, this.retainedFileCount);

        // Recorded only once the roll-over succeeded, so that a failed one is retried with the
        // next batch instead of being remembered as done.
        this.currentFilePath = filePath;
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