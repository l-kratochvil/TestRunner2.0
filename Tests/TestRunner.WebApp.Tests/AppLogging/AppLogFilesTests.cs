namespace TestRunner.WebApp.Tests.AppLogging;

using System.Globalization;
using NUnit.Framework;
using TestRunner.WebApp.Features.AppLogging.Services;

[TestFixture]
public class AppLogFilesTests
{
    private const int GivenRetainedFileCount = 5;

    private string logsDirectoryPath;

    [SetUp]
    public void SetUp()
    {
        this.logsDirectoryPath = Path.Combine(Path.GetTempPath(), $"applogging-files-{Guid.NewGuid():N}");
        Directory.CreateDirectory(this.logsDirectoryPath);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(this.logsDirectoryPath))
        {
            Directory.Delete(this.logsDirectoryPath, recursive: true);
        }
    }

    [TestCase("2026-08-26", ExpectedResult = "app-2026-08-26.log")]
    [TestCase("2025-01-01", ExpectedResult = "app-2025-01-01.log")]
    [TestCase("2026-12-31", ExpectedResult = "app-2026-12-31.log")]
    public string GetFilePath__WhenCalledWithATimestamp__ThenShouldReturnTheFileNamedAfterThatDay(
        string givenDate)
    {
        // Given:
        DateTimeOffset givenTimestamp = ParseDate(givenDate);

        // When:
        string result = AppLogFiles.GetFilePath(this.logsDirectoryPath, givenTimestamp);

        // Then:
        return Path.GetFileName(result);
    }

    [TestCaseSource(nameof(ApplyRetentionCases))]
    public string[] ApplyRetention__WhenCalledOnTheLogsDirectory__ThenShouldKeepOnlyTheNewestFiles(
        string[] givenExistingDates)
    {
        // Given:
        this.CreateLogFiles(givenExistingDates);

        // When:
        AppLogFiles.ApplyRetention(this.logsDirectoryPath, GivenRetainedFileCount);

        // Then:
        return this.GetLogFileNames();
    }

    [Test]
    public void ApplyRetention__WhenTheDirectoryDoesNotExist__ThenShouldDoNothing()
    {
        // Given:
        string givenMissingDirectoryPath = Path.Combine(this.logsDirectoryPath, "missing");

        // When:
        void ApplyRetention() => AppLogFiles.ApplyRetention(givenMissingDirectoryPath, GivenRetainedFileCount);

        // Then:
        Assert.DoesNotThrow(ApplyRetention);
    }

    [Test]
    public void ApplyRetention__WhenForeignFilesArePresent__ThenShouldLeaveThemAlone()
    {
        // Given:
        this.CreateLogFiles(
            ["2026-08-01", "2026-08-02", "2026-08-03", "2026-08-04", "2026-08-05", "2026-08-06"]);
        string givenForeignFilePath = Path.Combine(this.logsDirectoryPath, "notes.txt");
        File.WriteAllText(givenForeignFilePath, "not a log");

        // When:
        AppLogFiles.ApplyRetention(this.logsDirectoryPath, GivenRetainedFileCount);

        // Then:
        Assert.That(File.Exists(givenForeignFilePath), Is.True);
    }

    private static IEnumerable<TestCaseData> ApplyRetentionCases()
    {
        const string Prefix = nameof(AppLogFiles.ApplyRetention);

        // The array is wrapped in an object[] so that NUnit passes it as one argument instead of
        // spreading it over the parameter list.
        string[] manyFiles =
        [
            "2026-08-01", "2026-08-02", "2026-08-05", "2026-08-20", "2026-08-25", "2026-08-26",
        ];

        yield return new TestCaseData((object)manyFiles)
            .SetName(Prefix + "_WhenMoreFilesExistThanRetained_ThenShouldDeleteTheOldestOnes")
            .Returns(new[]
            {
                "app-2026-08-02.log",
                "app-2026-08-05.log",
                "app-2026-08-20.log",
                "app-2026-08-25.log",
                "app-2026-08-26.log",
            });

        // Days on which the application never ran leave no file behind and must not consume a slot.
        string[] distantFiles = ["2025-01-01", "2025-06-15", "2026-02-03", "2026-08-25", "2026-08-26"];

        yield return new TestCaseData((object)distantFiles)
            .SetName(Prefix + "_WhenDaysWithoutRunsExist_ThenShouldNotLetThemConsumeARetentionSlot")
            .Returns(new[]
            {
                "app-2025-01-01.log",
                "app-2025-06-15.log",
                "app-2026-02-03.log",
                "app-2026-08-25.log",
                "app-2026-08-26.log",
            });

        string[] fewFiles = ["2026-08-25", "2026-08-26"];

        yield return new TestCaseData((object)fewFiles)
            .SetName(Prefix + "_WhenFewerFilesExistThanRetained_ThenShouldDeleteNothing")
            .Returns(new[] { "app-2026-08-25.log", "app-2026-08-26.log" });
    }

    private static DateTimeOffset ParseDate(string date)
    {
        return DateTimeOffset.ParseExact(
            date,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal);
    }

    private void CreateLogFiles(string[] dates)
    {
        foreach (string date in dates)
        {
            File.WriteAllText(AppLogFiles.GetFilePath(this.logsDirectoryPath, ParseDate(date)), string.Empty);
        }
    }

    private string[] GetLogFileNames()
    {
        return [.. Directory
            .EnumerateFiles(this.logsDirectoryPath, AppLogFiles.FileSearchPattern)
            .Select(filePath => Path.GetFileName(filePath))
            .OrderBy(fileName => fileName, StringComparer.Ordinal)];
    }
}