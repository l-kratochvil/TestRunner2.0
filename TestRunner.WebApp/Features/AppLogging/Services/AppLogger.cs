namespace TestRunner.WebApp.Features.AppLogging.Services;

using TestRunner.WebApp.Features.AppLogging.Models;
using TestRunner.WebApp.Shared.Logging;

/// <summary>
/// Thin wrapper over <see cref="IAppLogStore"/> that stamps every entry with a fixed source.
/// </summary>
/// <param name="store">Store the entries are appended to.</param>
/// <param name="source">Source of the entries.</param>
public sealed class AppLogger(IAppLogStore store, string source) : IAppLogger
{
    /// <inheritdoc/>
    public string Source { get; } = source;

    /// <inheritdoc/>
    public void Debug(string message, string? detail = null)
    {
        this.Log(LogSeverity.Debug, message, detail);
    }

    /// <inheritdoc/>
    public void Info(string message, string? detail = null)
    {
        this.Log(LogSeverity.Info, message, detail);
    }

    /// <inheritdoc/>
    public void Warning(string message, string? detail = null)
    {
        this.Log(LogSeverity.Warning, message, detail);
    }

    /// <inheritdoc/>
    public void Error(string message, string? detail = null)
    {
        this.Log(LogSeverity.Error, message, detail);
    }

    /// <inheritdoc/>
    public void Log(LogSeverity severity, string message, string? detail = null)
    {
        store.Append(new LogEntry(DateTimeOffset.Now, severity, this.Source, message, detail));
    }
}