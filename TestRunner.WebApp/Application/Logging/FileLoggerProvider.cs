namespace TestRunner.WebApp.Application.Logging;

using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// An <see cref="ILoggerProvider"/> that writes diagnostics to the log file.
/// </summary>
/// <remarks>
/// Filtering stays with the logging pipeline through <c>Logging:File:LogLevel</c>.
/// </remarks>
[ProviderAlias("File")]
public sealed class FileLoggerProvider : IExtendedLoggerProvider
{
    private readonly ConcurrentDictionary<string, FileLogger> loggers = new(StringComparer.Ordinal);
    private readonly FileLogWriter writer;

    private readonly Lock gate = new();
    private Action<string>? failedHandlers;
    private string? failure;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileLoggerProvider"/> class.
    /// </summary>
    /// <param name="options">Options that configure the file logger.</param>
    public FileLoggerProvider(IOptions<FileLoggerOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        FileLoggerOptions value = options.Value;
        this.writer = new FileLogWriter(value.Path, value.RetainedFileCount);
        this.writer.Failed += this.OnWriterFailed;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// A handler added after an earlier failure still receives that failure, so startup breakage is
    /// never silent.
    /// </remarks>
    public event Action<string>? Failed
    {
        add
        {
            string? reportedFailure;

            lock (this.gate)
            {
                this.failedHandlers += value;
                reportedFailure = this.failure;
            }

            if (reportedFailure is not null)
            {
                value?.Invoke(reportedFailure);
            }
        }

        remove
        {
            lock (this.gate)
            {
                this.failedHandlers -= value;
            }
        }
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return this.loggers.GetOrAdd(categoryName, name => new FileLogger(name, this.writer));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.writer.Dispose();
    }

    private void OnWriterFailed(string message)
    {
        Action<string>? handlers;

        lock (this.gate)
        {
            this.failure = message;
            handlers = this.failedHandlers;
        }

        handlers?.Invoke(message);
    }
}