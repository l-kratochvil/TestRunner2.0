namespace TestRunner.WebApp.Application.Logging;

using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Writes every record the logging pipeline routes to it into a daily log file.
/// </summary>
/// <remarks>
/// Which records those are is decided by the standard <c>Logging:File:LogLevel</c> configuration,
/// so the provider itself never filters.
/// </remarks>
[ProviderAlias("File")]
public sealed class FileLoggerProvider : IExtendedLoggerProvider
{
    private readonly ConcurrentDictionary<string, FileLogger> loggers = new(StringComparer.Ordinal);
    private readonly FileLogWriter writer;

    private readonly Lock gate = new();
    private Action<string>? failedHandlers;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileLoggerProvider"/> class.
    /// </summary>
    /// <param name="options">Configuration of the log file.</param>
    public FileLoggerProvider(IOptions<FileLoggerOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        FileLoggerOptions value = options.Value;
        this.writer = new FileLogWriter(value.Path, value.RetainedFileCount);
        this.writer.Failed += this.OnWriterFailed;
    }

    /// <inheritdoc/>
    public event Action<string>? Failed
    {
        add
        {
            lock (this.gate)
            {
                this.failedHandlers += value;
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
            handlers = this.failedHandlers;
        }

        handlers?.Invoke(message);
    }
}