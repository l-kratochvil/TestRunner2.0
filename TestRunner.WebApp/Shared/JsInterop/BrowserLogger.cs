namespace TestRunner.WebApp.Shared.JsInterop;

using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

/// <summary>
/// Writes the diagnostics arriving from the browser into the logging pipeline.
/// </summary>
/// <remarks>
/// This is the .NET end of the bridge: the front-end holds a <c>DotNetObjectReference</c> to it and
/// calls <see cref="Log"/> over the circuit. Browser diagnostics reach the log file and nothing
/// else — a script fault is developer detail, so it never appears in the log the tester reads.
/// </remarks>
/// <param name="loggerFactory">Factory the category logger is created from.</param>
public sealed class BrowserLogger(ILoggerFactory loggerFactory)
{
    /// <summary>
    /// Logger category every browser diagnostic is written under.
    /// </summary>
    /// <remarks>
    /// One category for the whole front-end: it says where the code ran, which is the single thing
    /// that separates these records from the ones written on the test machine. The script the
    /// record came from is a field of the entry instead, because a category per script would have
    /// to be configured per script too.
    /// </remarks>
    public const string Category = "TestRunner.WebApp.Browser";

    private readonly ILogger logger = loggerFactory.CreateLogger(Category);

    /// <summary>
    /// Writes one diagnostic coming from the browser.
    /// </summary>
    /// <param name="diagnostic">The diagnostic to write.</param>
    [JSInvokable]
    public void Log(BrowserDiagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);

        LogLevel level = GetLevel(diagnostic.Level);

        if (string.IsNullOrEmpty(diagnostic.Detail))
        {
            this.logger.Log(level, "[{Module}] {Message}", diagnostic.Module, diagnostic.Message);

            return;
        }

        // The detail stays a value of its own instead of being concatenated into the message, so
        // that the message template keeps its meaning for structured destinations. Destinations
        // rendering plain text see it as the lines following the message.
        this.logger.Log(
            level,
            "[{Module}] {Message}\n{Detail}",
            diagnostic.Module,
            diagnostic.Message,
            diagnostic.Detail);
    }

    /// <summary>
    /// Maps the severity as the browser spells it onto the pipeline's own.
    /// </summary>
    /// <remarks>
    /// An unrecognized severity is a fault of the calling script, so the record is kept and raised
    /// to <see cref="LogLevel.Warning"/>: dropping it would hide both the record and the fault, and
    /// passing it off as information would hide only the fault.
    /// </remarks>
    /// <param name="level">Severity as the browser spells it.</param>
    /// <returns>The level the record is written at.</returns>
    private static LogLevel GetLevel(string? level)
        => level?.ToLowerInvariant() switch
        {
            "debug" => LogLevel.Debug,
            "info" => LogLevel.Information,
            "warn" => LogLevel.Warning,
            "error" => LogLevel.Error,
            _ => LogLevel.Warning,
        };
}