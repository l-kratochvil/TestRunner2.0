namespace TestRunner.WebApp.Tests.AppLogging;

using NUnit.Framework;
using TestRunner.WebApp.Features.AppLogging.Models;
using TestRunner.WebApp.Features.AppLogging.Services;
using TestRunner.WebApp.Shared.Logging;

[TestFixture]
public class AppLogFileSinkTests
{
    private const int GivenRetainedFileCount = 5;

    private string logsDirectoryPath;
    private string? blockingFilePath;
    private AppLogFileSink unit;

    [SetUp]
    public void SetUp()
    {
        this.logsDirectoryPath = Path.Combine(Path.GetTempPath(), $"applogging-sink-{Guid.NewGuid():N}");
        this.blockingFilePath = null;
        this.unit = new AppLogFileSink(new AppLoggingOptions
        {
            LogsDirectoryPath = this.logsDirectoryPath,
            RetainedFileCount = GivenRetainedFileCount,
        });
    }

    [TearDown]
    public void TearDown()
    {
        this.unit.Dispose();

        if (Directory.Exists(this.logsDirectoryPath))
        {
            Directory.Delete(this.logsDirectoryPath, recursive: true);
        }

        if (this.blockingFilePath is not null && File.Exists(this.blockingFilePath))
        {
            File.Delete(this.blockingFilePath);
        }
    }

    [Test]
    public async Task StartAsync__WhenCalled__ThenShouldCreateTodaysLogFile()
    {
        // When:
        await this.unit.StartAsync(CancellationToken.None);
        await this.unit.StopAsync(CancellationToken.None);

        // Then:
        Assert.That(File.Exists(this.unit.CurrentFilePath), Is.True);
    }

    [Test]
    public async Task StartAsync__WhenMoreLogFilesExistThanRetained__ThenShouldDeleteTheOldestOnes()
    {
        // Given:
        const int givenExistingFileCount = 10;
        Directory.CreateDirectory(this.logsDirectoryPath);
        foreach (int daysBack in Enumerable.Range(1, givenExistingFileCount))
        {
            File.WriteAllText(
                AppLogFiles.GetFilePath(this.logsDirectoryPath, DateTimeOffset.Now.AddDays(-daysBack)),
                string.Empty);
        }

        // When:
        await this.unit.StartAsync(CancellationToken.None);
        await this.unit.StopAsync(CancellationToken.None);

        // Then:
        Assert.That(
            Directory.EnumerateFiles(this.logsDirectoryPath, AppLogFiles.FileSearchPattern).Count(),
            Is.EqualTo(GivenRetainedFileCount));
    }

    [Test]
    public async Task StopAsync__WhenEntriesArePending__ThenShouldFlushThemToTodaysLogFile()
    {
        // Given:
        var givenEntries = new[]
        {
            new LogEntry(DateTimeOffset.Now, LogSeverity.Debug, LogSources.TestRun, "raw output"),
            new LogEntry(DateTimeOffset.Now, LogSeverity.Error, LogSources.App, "failed", "detail line"),
        };

        await this.unit.StartAsync(CancellationToken.None);
        string expectedFilePath = this.unit.CurrentFilePath;

        foreach (LogEntry entry in givenEntries)
        {
            this.unit.Write(entry);
        }

        // When:
        await this.unit.StopAsync(CancellationToken.None);

        // Then:
        string content = File.ReadAllText(expectedFilePath).ReplaceLineEndings("\n");
        using (Assert.EnterMultipleScope())
        {
            // Debug entries are kept on disk even though the panel hides them by default.
            Assert.That(content, Does.Contain("[Debug  ] [TestRun] raw output"));
            Assert.That(content, Does.Contain("[Error  ] [App] failed"));
            Assert.That(content, Does.Contain("\n    detail line\n"));
        }
    }

    [Test]
    public async Task StartAsync__WhenTheLogsDirectoryCannotBeCreated__ThenShouldReportTheFailureOnce()
    {
        // Given:
        var reportedFailures = new List<string>();
        this.unit = this.CreateUnitWithBlockedLogsDirectory();
        this.unit.Failed += reportedFailures.Add;

        // When:
        await this.unit.StartAsync(CancellationToken.None);
        this.unit.Write(new LogEntry(DateTimeOffset.Now, LogSeverity.Info, LogSources.App, "still alive"));
        await this.unit.StopAsync(CancellationToken.None);

        // Then:
        Assert.That(reportedFailures, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Write__WhenTheLogsDirectoryCannotBeCreated__ThenShouldNotThrow()
    {
        // Given:
        this.unit = this.CreateUnitWithBlockedLogsDirectory();
        await this.unit.StartAsync(CancellationToken.None);

        // When:
        void WriteAndStop()
        {
            this.unit.Write(new LogEntry(DateTimeOffset.Now, LogSeverity.Info, LogSources.App, "still alive"));
            this.unit.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        // Then:
        Assert.DoesNotThrow(WriteAndStop);
    }

    [Test]
    public void Dispose__WhenCalledRepeatedly__ThenShouldNotThrow()
    {
        // The same instance is registered as the sink and as a hosted service, so the container
        // disposes it more than once.

        // When:
        void DisposeTwice()
        {
            this.unit.Dispose();
            this.unit.Dispose();
        }

        // Then:
        Assert.DoesNotThrow(DisposeTwice);
    }

    private AppLogFileSink CreateUnitWithBlockedLogsDirectory()
    {
        // A file where the directory should be makes every directory operation fail.
        this.blockingFilePath = Path.Combine(Path.GetTempPath(), $"applogging-blocked-{Guid.NewGuid():N}");
        File.WriteAllText(this.blockingFilePath, "not a directory");

        this.unit.Dispose();
        return new AppLogFileSink(new AppLoggingOptions { LogsDirectoryPath = this.blockingFilePath });
    }
}