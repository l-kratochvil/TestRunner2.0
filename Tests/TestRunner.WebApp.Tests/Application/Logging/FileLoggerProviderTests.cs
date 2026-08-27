namespace TestRunner.WebApp.Tests.Application.Logging;

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using TestRunner.WebApp.Application.Logging;

[TestFixture]
public class FileLoggerProviderTests
{
    private const string GivenCategory = "TestRunner.AppLog.App";

    private string directoryPath;

    [SetUp]
    public void SetUp()
    {
        this.directoryPath = Path.Combine(Path.GetTempPath(), $"filelogger-{Guid.NewGuid():N}");
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(this.directoryPath))
        {
            Directory.Delete(this.directoryPath, recursive: true);
        }
        else if (File.Exists(this.directoryPath))
        {
            File.Delete(this.directoryPath);
        }
    }

    [Test]
    public void CreateLogger__WhenAnEntryIsLogged__ThenShouldWriteItIntoTodaysFile()
    {
        // Given:
        const string givenMessage = "Application started.";

        // When:
        this.LogAndFlush(logger => logger.LogInformation("{Message}", givenMessage));

        // Then:
        string[] lines = this.ReadTodaysFile();
        Assert.That(
            lines.Single(),
            Does.Match(
                @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} \[INFO \] \["
                + Regex.Escape(GivenCategory)
                + @"\] "
                + Regex.Escape(givenMessage)
                + "$"));
    }

    [TestCase(LogLevel.Trace, "TRACE")]
    [TestCase(LogLevel.Debug, "DEBUG")]
    [TestCase(LogLevel.Information, "INFO ")]
    [TestCase(LogLevel.Warning, "WARN ")]
    [TestCase(LogLevel.Error, "ERROR")]
    [TestCase(LogLevel.Critical, "CRIT ")]
    public void CreateLogger__WhenAnEntryIsLoggedAtALevel__ThenShouldWriteThatLevelInAFixedWidthColumn(
        LogLevel givenLevel, string expectedName)
    {
        // When:
        this.LogAndFlush(logger => logger.Log(givenLevel, "{Message}", "message"));

        // Then:
        Assert.That(this.ReadTodaysFile().Single(), Does.Contain($"[{expectedName}] "));
    }

    [Test]
    public void CreateLogger__WhenTheMessageSpansSeveralLines__ThenShouldIndentEveryLineButTheFirst()
    {
        // Given:
        string[] expectedLines =
        [
            "Test run finished.",
            "    line 1",
            "    line 2",
        ];

        // When:
        this.LogAndFlush(logger =>
            logger.LogInformation("{Message}\n{Detail}", "Test run finished.", "line 1\nline 2"));

        // Then:
        string[] lines = this.ReadTodaysFile();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(lines, Has.Length.EqualTo(expectedLines.Length));
            Assert.That(lines[0], Does.EndWith(expectedLines[0]));
            Assert.That(lines[1], Is.EqualTo(expectedLines[1]));
            Assert.That(lines[2], Is.EqualTo(expectedLines[2]));
        }
    }

    [Test]
    public void CreateLogger__WhenAnExceptionIsAttached__ThenShouldWriteItAsIndentedDetail()
    {
        // Given:
        var givenException = new InvalidOperationException("boom");

        // When:
        this.LogAndFlush(logger => logger.LogError(givenException, "{Message}", "failed"));

        // Then:
        string[] lines = this.ReadTodaysFile();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(lines[0], Does.EndWith("failed"));
            Assert.That(lines[1], Does.StartWith("    ").And.Contains("boom"));
        }
    }

    [Test]
    public void CreateLogger__WhenTheSameCategoryIsAskedForTwice__ThenShouldReturnTheSameLogger()
    {
        // Given:
        using FileLoggerProvider unit = this.CreateUnit();

        // When:
        ILogger result = unit.CreateLogger(GivenCategory);

        // Then:
        Assert.That(result, Is.SameAs(unit.CreateLogger(GivenCategory)));
    }

    [Test]
    public void CreateLogger__WhenOlderFilesExist__ThenShouldApplyRetentionOnTheFirstWrite()
    {
        // Given:
        // Retention runs on the first write and not in the constructor, so that a failure of it
        // can still reach a handler attached after the provider was created.
        Directory.CreateDirectory(this.directoryPath);
        string[] givenOldFileNames =
            ["2020-01-01.log", "2020-01-02.log", "2020-01-03.log", "2020-01-04.log", "2020-01-05.log"];

        foreach (string fileName in givenOldFileNames)
        {
            File.WriteAllText(Path.Combine(this.directoryPath, fileName), string.Empty);
        }

        // When:
        this.LogAndFlush(logger => logger.LogInformation("{Message}", "message"), retainedFileCount: 3);

        // Then:
        // Today's file counts as one of the retained files, so only the two newest old ones stay.
        Assert.That(
            Directory.EnumerateFiles(this.directoryPath).Select(path => Path.GetFileName(path)).Order(StringComparer.Ordinal),
            Is.EqualTo(new[]
            {
                "2020-01-04.log",
                "2020-01-05.log",
                Path.GetFileName(LogFile.GetPath(this.directoryPath, DateTimeOffset.Now)),
            }));
    }

    [Test]
    public void Failed__WhenTheDirectoryCannotBeCreated__ThenShouldReportItToASubscriber()
    {
        // Given:
        // A file where the directory should be: creating the directory fails, and the log has to
        // say so instead of silently disappearing.
        this.BlockTheDirectory();
        var reported = new List<string>();

        using (FileLoggerProvider unit = this.CreateUnit())
        {
            unit.Failed += reported.Add;

            // When:
            unit.CreateLogger(GivenCategory).LogInformation("{Message}", "message");
        }

        // Then:
        Assert.That(reported.Single(), Does.Contain("log file"));
    }

    [Test]
    public void Failed__WhenTheSubscriberAttachesAfterTheFailure__ThenShouldReportItImmediately()
    {
        // Given:
        // The provider is created with the logging pipeline, long before whoever surfaces the
        // failure to the user exists.
        this.BlockTheDirectory();
        var reported = new List<string>();

        using FileLoggerProvider unit = this.CreateUnit();
        unit.CreateLogger(GivenCategory).LogInformation("{Message}", "message");
        SpinWaitForFailure(unit);

        // When:
        unit.Failed += reported.Add;

        // Then:
        Assert.That(reported.Single(), Does.Contain("log file"));
    }

    [Test]
    public void Failed__WhenTheFailureIsNotAFileSystemOne__ThenShouldStillReportIt()
    {
        // Given:
        // An empty path makes Directory.CreateDirectory throw ArgumentException, which is not one
        // of the file system exceptions the writer used to expect. It escaped the loop, killed the
        // background writer for good and reported nothing: the log went silent while looking
        // healthy, and the undrained queue kept growing.
        var reported = new List<string>();

        using (var unit = new FileLoggerProvider(Options.Create(new FileLoggerOptions
        {
            Path = string.Empty,
            RetainedFileCount = 5,
        })))
        {
            unit.Failed += reported.Add;

            // When:
            unit.CreateLogger(GivenCategory).LogInformation("{Message}", "message");
        }

        // Then:
        Assert.That(reported.Single(), Does.Contain("log file"));
    }

    [Test]
    public void CreateLogger__WhenAnEarlierWriteFailed__ThenShouldKeepWritingOnceTheDirectoryWorksAgain()
    {
        // Given:
        // The loop has to survive a failure. It also has to retry the roll-over, because the first
        // attempt never reached the retention that a successful one performs.
        this.BlockTheDirectory();

        using (FileLoggerProvider unit = this.CreateUnit())
        {
            ILogger logger = unit.CreateLogger(GivenCategory);
            logger.LogInformation("{Message}", "lost");
            SpinWaitForFailure(unit);

            File.Delete(this.directoryPath);

            // When:
            logger.LogInformation("{Message}", "written");
        }

        // Then:
        Assert.That(this.ReadTodaysFile().Single(), Does.EndWith("written"));
    }

    [Test]
    public void Failed__WhenWritingKeepsFailing__ThenShouldReportItOnlyOnce()
    {
        // Given:
        this.BlockTheDirectory();
        var reported = new List<string>();

        using (FileLoggerProvider unit = this.CreateUnit())
        {
            unit.Failed += reported.Add;
            ILogger logger = unit.CreateLogger(GivenCategory);

            // When:
            for (int i = 0; i < 5; i++)
            {
                logger.LogInformation("{Message}", $"message {i}");
            }
        }

        // Then:
        Assert.That(reported, Has.Count.EqualTo(1));
    }

    [Test]
    public void Dispose__WhenCalledTwice__ThenShouldNotThrow()
    {
        // Given:
        // The logger factory and the container both dispose the provider.
        FileLoggerProvider unit = this.CreateUnit();
        unit.CreateLogger(GivenCategory).LogInformation("{Message}", "message");

        // When:
        unit.Dispose();

        // Then:
        Assert.That(unit.Dispose, Throws.Nothing);
    }

    private static void SpinWaitForFailure(FileLoggerProvider provider)
    {
        // The record is written by a background loop, so the failure is not observable at once.
        var observed = false;
        provider.Failed += _ => observed = true;

        SpinWait.SpinUntil(() => observed, TimeSpan.FromSeconds(5));
    }

    private FileLoggerProvider CreateUnit(int retainedFileCount = 5)
    {
        return new FileLoggerProvider(Options.Create(new FileLoggerOptions
        {
            Path = this.directoryPath,
            RetainedFileCount = retainedFileCount,
        }));
    }

    /// <summary>
    /// Logs through the provider and disposes it, because disposing is what flushes the pending
    /// records to disk.
    /// </summary>
    private void LogAndFlush(Action<ILogger> log, int retainedFileCount = 5)
    {
        using FileLoggerProvider unit = this.CreateUnit(retainedFileCount);
        log(unit.CreateLogger(GivenCategory));
    }

    private void BlockTheDirectory()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(this.directoryPath)!);
        File.WriteAllText(this.directoryPath, string.Empty);
    }

    private string[] ReadTodaysFile()
    {
        return File.ReadAllLines(LogFile.GetPath(this.directoryPath, DateTimeOffset.Now));
    }
}