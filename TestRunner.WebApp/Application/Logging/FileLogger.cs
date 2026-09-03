namespace TestRunner.WebApp.Application.Logging;

using Microsoft.Extensions.Logging;

/// <summary>
/// An <see cref="ILogger"/> that writes one logger category into the log file.
/// </summary>
/// <param name="category">Logger category written by this instance.</param>
/// <param name="writer">Writer the entries are handed to.</param>
internal sealed class FileLogger(string category, FileLogWriter writer) : ILogger
{
    /// <inheritdoc/>
    /// <remarks>
    /// Scopes do not reach the log file.
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