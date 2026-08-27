namespace TestRunner.WebApp.Application.Logging;

using Microsoft.Extensions.Logging;

/// <summary>
/// Logger of one category writing into the log file.
/// </summary>
/// <param name="category">Category the logger was created for.</param>
/// <param name="writer">Writer the records are handed to.</param>
internal sealed class FileLogger(string category, FileLogWriter writer) : ILogger
{
    /// <inheritdoc/>
    /// <remarks>
    /// Scopes are not written to the file, so there is nothing to push or pop.
    /// </remarks>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        if (!this.IsEnabled(logLevel))
        {
            return;
        }

        // The timestamp is taken here and not in the writer: the record is written asynchronously
        // and must still be dated when it happened, including when it lands on the far side of
        // midnight.
        writer.Write(new FileLogEntry(
            DateTimeOffset.Now,
            logLevel,
            category,
            formatter(state, exception),
            exception));
    }
}