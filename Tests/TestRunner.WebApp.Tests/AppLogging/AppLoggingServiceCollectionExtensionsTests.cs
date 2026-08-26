namespace TestRunner.WebApp.Tests.AppLogging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using TestRunner.WebApp.Features.AppLogging.Services;
using TestRunner.WebApp.Shared.Logging;

[TestFixture]
public class AppLoggingServiceCollectionExtensionsTests
{
    private ServiceProvider unit;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        services.AddAppLogging(new AppLoggingOptions
        {
            LogsDirectoryPath = Path.Combine(Path.GetTempPath(), $"applogging-di-{Guid.NewGuid():N}"),
        });

        this.unit = services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        this.unit.Dispose();
    }

    [Test]
    public void AddAppLogging__WhenTheDefaultLoggerIsResolved__ThenShouldBeBoundToTheAppSource()
    {
        // When:
        var result = this.unit.GetRequiredService<IAppLogger>();

        // Then:
        Assert.That(result.Source, Is.EqualTo(LogSources.App));
    }

    [Test]
    public void AddAppLogging__WhenLoggersOfDifferentSourcesLog__ThenShouldAppendIntoTheSameStore()
    {
        // Given:
        string[] expectedSources = [LogSources.TestRun, LogSources.TestLink];
        var givenFactory = this.unit.GetRequiredService<IAppLoggerFactory>();

        // When:
        givenFactory.CreateLogger(expectedSources[0]).Warning("slow");
        givenFactory.CreateLogger(expectedSources[1]).Error("unreachable");

        // Then:
        Assert.That(
            this.unit.GetRequiredService<IAppLogStore>().GetEntries().Select(entry => entry.Source),
            Is.EqualTo(expectedSources));
    }

    [Test]
    public void AddAppLogging__WhenHostedServicesAreResolved__ThenShouldStartTheLifecycleLoggerAfterTheFileSink()
    {
        // Given:
        // Hosted services stop in reverse order, so the "shutting down" entry only reaches the
        // file if the lifecycle logger is registered after the sink.
        Type[] expectedTypes = [typeof(AppLogFileSink), typeof(AppLifecycleLogger)];

        // When:
        IEnumerable<Type> result = this.unit
            .GetServices<IHostedService>()
            .Select(service => service.GetType());

        // Then:
        Assert.That(result, Is.EqualTo(expectedTypes));
    }

    [Test]
    public void AddAppLogging__WhenTheFileSinkIsResolvedThroughEveryRegistration__ThenShouldReturnTheSameInstance()
    {
        // Given:
        var expectedSink = this.unit.GetRequiredService<AppLogFileSink>();

        // When:
        var sinkRegistration = this.unit.GetServices<IAppLogSink>().Single();
        var hostedServiceRegistration = this.unit.GetServices<IHostedService>().OfType<AppLogFileSink>().Single();

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(sinkRegistration, Is.SameAs(expectedSink));
            Assert.That(hostedServiceRegistration, Is.SameAs(expectedSink));
        });
    }
}