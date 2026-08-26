namespace TestRunner.WebApp.Tests.AppLogging;

using NUnit.Framework;
using TestRunner.WebApp.Features.AppLogging.Models;
using TestRunner.WebApp.Features.AppLogging.Services;
using TestRunner.WebApp.Shared.Logging;

[TestFixture]
public class AppLogFileFormatterTests
{
    private static readonly DateTimeOffset GivenTimestamp =
        new(2026, 8, 26, 8, 39, 12, 345, TimeSpan.Zero);

    [TestCase(
        LogSeverity.Debug,
        LogSources.TestRun,
        "raw output",
        null,
        ExpectedResult = "2026-08-26 08:39:12.345 [Debug  ] [TestRun] raw output\n")]
    [TestCase(
        LogSeverity.Info,
        LogSources.TestRun,
        "Test run started.",
        null,
        ExpectedResult = "2026-08-26 08:39:12.345 [Info   ] [TestRun] Test run started.\n")]
    [TestCase(
        LogSeverity.Warning,
        LogSources.TestLink,
        "Slow response.",
        null,
        ExpectedResult = "2026-08-26 08:39:12.345 [Warning] [TestLink] Slow response.\n")]
    [TestCase(
        LogSeverity.Error,
        LogSources.App,
        "Failed.",
        "line 1\nline 2",
        ExpectedResult = "2026-08-26 08:39:12.345 [Error  ] [App] Failed.\n    line 1\n    line 2\n")]
    public string Format__WhenCalledWithAnEntry__ThenShouldReturnOneLinePerEntryWithIndentedDetail(
        LogSeverity givenSeverity, string givenSource, string givenMessage, string? givenDetail)
    {
        // Given:
        var givenEntry = new LogEntry(GivenTimestamp, givenSeverity, givenSource, givenMessage, givenDetail);

        // When:
        string result = AppLogFileFormatter.Format(givenEntry);

        // Then:
        return result.ReplaceLineEndings("\n");
    }
}