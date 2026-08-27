namespace TestRunner.WebApp.Tests.AppLogging;

using Moq;
using NUnit.Framework;
using TestRunner.WebApp.Features.AppLogging.Models;
using TestRunner.WebApp.Features.AppLogging.Services;
using TestRunner.WebApp.Shared.Logging;

[TestFixture]
public class AppLogStoreTests
{
    private Mock<IAppLogSink> sinkMock;
    private AppLogStore unit;

    [SetUp]
    public void SetUp()
    {
        this.sinkMock = new Mock<IAppLogSink>();
        this.unit = new AppLogStore(new AppLoggingOptions(), [this.sinkMock.Object]);
    }

    [Test]
    public void Append__WhenCapacityIsExceeded__ThenShouldDropTheOldestEntries()
    {
        // Given:
        const int givenCapacity = 3;
        const int givenEntryCount = 5;
        string[] expectedMessages = ["entry 3", "entry 4", "entry 5"];
        this.unit = new AppLogStore(new AppLoggingOptions { Capacity = givenCapacity }, []);

        // When:
        for (int i = 1; i <= givenEntryCount; i++)
        {
            this.unit.Append(CreateEntry($"entry {i}"));
        }

        // Then:
        Assert.That(this.unit.GetEntries().Select(entry => entry.Message), Is.EqualTo(expectedMessages));
    }

    [Test]
    public void GetEntries__WhenEntriesWereAppended__ThenShouldReturnThemOldestFirst()
    {
        // Given:
        string[] expectedMessages = ["first", "second"];

        // When:
        this.unit.Append(CreateEntry(expectedMessages[0]));
        this.unit.Append(CreateEntry(expectedMessages[1]));

        // Then:
        Assert.That(this.unit.GetEntries().Select(entry => entry.Message), Is.EqualTo(expectedMessages));
    }

    [Test]
    public void Append__WhenCalledConcurrently__ThenShouldKeepEveryEntry()
    {
        // Given:
        const int givenEntryCount = 1000;
        this.unit = new AppLogStore(new AppLoggingOptions { Capacity = givenEntryCount }, []);

        // When:
        Parallel.For(0, givenEntryCount, i => this.unit.Append(CreateEntry($"entry {i}")));

        // Then:
        Assert.That(this.unit.GetEntries(), Has.Count.EqualTo(givenEntryCount));
    }

    [Test]
    public void Append__WhenSinkIsRegistered__ThenShouldHandTheEntryToIt()
    {
        // Given:
        LogEntry givenEntry = CreateEntry("mirrored");

        // When:
        this.unit.Append(givenEntry);

        // Then:
        this.sinkMock.Verify(sink => sink.Write(givenEntry), Times.Once);
    }

    [Test]
    public void Append__WhenEntryIsAppended__ThenShouldNotifySubscribers()
    {
        // Given:
        const string givenMessage = "observed";
        var observedMessages = new List<string>();
        this.unit.EntryAppended += entry => observedMessages.Add(entry.Message);

        // When:
        this.unit.Append(CreateEntry(givenMessage));

        // Then:
        Assert.That(observedMessages, Is.EqualTo(new[] { givenMessage }));
    }

    [Test]
    public void Failed__WhenSinkReportsFailure__ThenShouldBufferItAsAnErrorOfTheAppSource()
    {
        // Given:
        const string givenFailureMessage = "disk is full";

        // When:
        this.sinkMock.Raise(sink => sink.Failed += null, givenFailureMessage);

        // Then:
        LogEntry failure = this.unit.GetEntries().Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(failure.Severity, Is.EqualTo(LogSeverity.Error));
            Assert.That(failure.Source, Is.EqualTo(LogSources.App));
            Assert.That(failure.Message, Is.EqualTo(givenFailureMessage));
        }
    }

    [Test]
    public void Failed__WhenSinkReportsFailure__ThenShouldNotRouteTheFailureBackToTheSinks()
    {
        // Given:
        const string givenFailureMessage = "disk is full";

        // When:
        this.sinkMock.Raise(sink => sink.Failed += null, givenFailureMessage);

        // Then:
        this.sinkMock.Verify(sink => sink.Write(It.IsAny<LogEntry>()), Times.Never);
    }

    private static LogEntry CreateEntry(string message)
    {
        return new LogEntry(DateTimeOffset.Now, LogSeverity.Info, LogSources.App, message);
    }
}