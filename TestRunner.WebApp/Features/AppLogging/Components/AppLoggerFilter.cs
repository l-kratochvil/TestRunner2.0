namespace TestRunner.WebApp.Features.AppLogging.Components;

using TestRunner.WebApp.Features.AppLogging.Models;
using TestRunner.WebApp.Shared.Logging;

/// <summary>
/// Selection of severities and sources shown in the log panel.
/// </summary>
/// <remarks>
/// Severities are tracked as what is <em>shown</em>, because they are known up front. Sources are
/// tracked the other way round, as what is <em>hidden</em>, so that a source that is not known up
/// front (see <see cref="LogSources.All"/>) is visible instead of being silently dropped.
/// </remarks>
public sealed class AppLoggerFilter
{
    private readonly HashSet<LogSeverity> selectedSeverities;
    private readonly HashSet<string> hiddenSources = new(StringComparer.OrdinalIgnoreCase);

    private AppLoggerFilter(IEnumerable<LogSeverity> selectedSeverities)
    {
        this.selectedSeverities = [.. selectedSeverities];
    }

    /// <summary>
    /// Creates the filter used when the panel is first shown: every severity and every source.
    /// </summary>
    /// <returns>The default filter.</returns>
    public static AppLoggerFilter CreateDefault()
    {
        return new AppLoggerFilter(Enum.GetValues<LogSeverity>());
    }

    /// <summary>
    /// Determines whether entries of the given severity are shown.
    /// </summary>
    /// <param name="severity">Severity to test.</param>
    /// <returns><see langword="true"/> when the severity is shown.</returns>
    public bool IsSelected(LogSeverity severity)
    {
        return this.selectedSeverities.Contains(severity);
    }

    /// <summary>
    /// Determines whether entries of the given source are shown.
    /// </summary>
    /// <param name="source">Source to test.</param>
    /// <returns><see langword="true"/> when the source is shown.</returns>
    public bool IsSelected(string source)
    {
        return !this.hiddenSources.Contains(source);
    }

    /// <summary>
    /// Shows or hides entries of the given severity.
    /// </summary>
    /// <param name="severity">Severity to change.</param>
    /// <param name="selected">Whether the severity should be shown.</param>
    public void SetSelected(LogSeverity severity, bool selected)
    {
        if (selected)
        {
            this.selectedSeverities.Add(severity);
        }
        else
        {
            this.selectedSeverities.Remove(severity);
        }
    }

    /// <summary>
    /// Shows or hides entries of the given source.
    /// </summary>
    /// <param name="source">Source to change.</param>
    /// <param name="selected">Whether the source should be shown.</param>
    public void SetSelected(string source, bool selected)
    {
        if (selected)
        {
            this.hiddenSources.Remove(source);
        }
        else
        {
            this.hiddenSources.Add(source);
        }
    }

    /// <summary>
    /// Determines whether the entry passes the filter.
    /// </summary>
    /// <param name="entry">Entry to test.</param>
    /// <returns><see langword="true"/> when the entry should be shown.</returns>
    public bool Matches(LogEntry entry)
    {
        return this.IsSelected(entry.Severity) && this.IsSelected(entry.Source);
    }

    /// <summary>
    /// Filters the entries.
    /// </summary>
    /// <param name="entries">Entries to filter.</param>
    /// <returns>Entries passing the filter, in the original order.</returns>
    public IEnumerable<LogEntry> Apply(IEnumerable<LogEntry> entries)
    {
        return entries.Where(this.Matches);
    }
}