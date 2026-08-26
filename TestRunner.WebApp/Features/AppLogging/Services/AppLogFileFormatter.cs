namespace TestRunner.WebApp.Features.AppLogging.Services;

using System.Globalization;
using System.Text;
using TestRunner.WebApp.Features.AppLogging.Models;

/// <summary>
/// Renders log entries as the plain text lines written to the log file.
/// </summary>
public static class AppLogFileFormatter
{
    private const string DetailIndent = "    ";

    /// <summary>
    /// Formats the entry as one line, followed by its indented detail lines.
    /// </summary>
    /// <param name="entry">Entry to format.</param>
    /// <returns>The formatted entry, terminated by a new line.</returns>
    public static string Format(LogEntry entry)
    {
        StringBuilder builder = new();
        AppendTo(builder, entry);
        return builder.ToString();
    }

    /// <summary>
    /// Appends the formatted entry to the builder.
    /// </summary>
    /// <param name="builder">Builder to append to.</param>
    /// <param name="entry">Entry to format.</param>
    public static void AppendTo(StringBuilder builder, LogEntry entry)
    {
        builder
            .Append(entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture))
            .Append(" [")
            .Append(entry.Severity.ToString().PadRight(7))
            .Append("] [")
            .Append(entry.Source)
            .Append("] ")
            .AppendLine(entry.Message);

        if (string.IsNullOrEmpty(entry.Detail))
        {
            return;
        }

        foreach (string line in entry.Detail.ReplaceLineEndings("\n").Split('\n'))
        {
            builder.Append(DetailIndent).AppendLine(line);
        }
    }
}