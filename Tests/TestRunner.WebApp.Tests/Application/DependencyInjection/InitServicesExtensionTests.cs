namespace TestRunner.WebApp.Tests.Application.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using TestRunner.WebApp.Application.DependencyInjection;
using TestRunner.WebApp.Application.Logging;
using TestRunner.WebApp.Features.AppLogging.Services;
using TestRunner.WebApp.Shared.JsInterop;
using TestRunner.WebApp.Shared.Logging;

[TestFixture]
public class InitServicesExtensionTests
{
    private string logsDirectoryPath;
    private ServiceProvider unit;

    [SetUp]
    public void SetUp()
    {
        this.logsDirectoryPath = Path.Combine(Path.GetTempPath(), $"applogging-di-{Guid.NewGuid():N}");
        this.unit = BuildProvider(this.logsDirectoryPath);
    }

    [TearDown]
    public void TearDown()
    {
        this.unit.Dispose();

        if (Directory.Exists(this.logsDirectoryPath))
        {
            Directory.Delete(this.logsDirectoryPath, recursive: true);
        }
    }

    [Test]
    public void InitAppLogging__WhenTheDefaultLoggerIsResolved__ThenShouldBeBoundToTheAppSource()
    {
        // When:
        var result = this.unit.GetRequiredService<IAppLogger>();

        // Then:
        Assert.That(result.Source, Is.EqualTo(LogSources.App));
    }

    [Test]
    public void InitAppLogging__WhenLoggersOfDifferentSourcesLog__ThenShouldAppendIntoTheSameStore()
    {
        // Given:
        string[] expectedSources = [LogSources.TestRun, LogSources.TestLink];
        var givenFactory = this.unit.GetRequiredService<IAppLoggerFactory>();

        // When:
        givenFactory.CreateLogger(expectedSources[0]).Warning("slow");
        givenFactory.CreateLogger(expectedSources[1]).Error("unreachable");

        // Then:
        Assert.That(
            this.unit.GetRequiredService<IAppLoggerStore>().GetEntries().Select(entry => entry.Source),
            Is.EqualTo(expectedSources));
    }

    [Test]
    public void InitAppLogging__WhenTheSinksAreResolved__ThenShouldRegisterTheBridgeIntoTheLoggingPipeline()
    {
        // Given:
        Type[] expectedTypes = [typeof(DiagnosticsLoggerSink)];

        // When:
        IEnumerable<Type> result = this.unit.GetServices<IAppLoggerSink>().Select(sink => sink.GetType());

        // Then:
        Assert.That(result, Is.EqualTo(expectedTypes));
    }

    [Test]
    public void InitAppLogging__WhenHostedServicesAreResolved__ThenShouldRegisterNone()
    {
        // Given:
        // The log file is owned by the logging pipeline now, so it no longer needs a lifecycle of
        // its own.

        // When:
        IEnumerable<IHostedService> result = this.unit.GetServices<IHostedService>();

        // Then:
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void InitAppLogging__WhenAnEntryIsLogged__ThenShouldReachTheLogFileThroughTheLoggingPipeline()
    {
        // Given:
        const string givenMessage = "Test run finished.";
        this.unit.GetRequiredService<IAppLogger>().Info(givenMessage);

        // When:
        // Disposing flushes the pending records of the file provider.
        this.unit.Dispose();

        // Then:
        string content = File.ReadAllText(LogFile.GetPath(this.logsDirectoryPath, DateTimeOffset.Now));
        Assert.That(content, Does.Contain(DiagnosticsLoggerSink.GetCategory(LogSources.App)).And.Contains(givenMessage));
    }

    [Test]
    public void InitAppLogging__WhenTheLogFileCannotBeWritten__ThenShouldReportItInTheStore()
    {
        // Given:
        // A file where the logs directory should be, so that the provider cannot write anything.
        // A silently broken log file is the worst way for a log to fail, so it has to surface in
        // the panel.
        string givenBlockedPath = Path.Combine(Path.GetTempPath(), $"applogging-di-blocked-{Guid.NewGuid():N}");
        File.WriteAllText(givenBlockedPath, string.Empty);

        try
        {
            ServiceProvider provider = BuildProvider(givenBlockedPath);
            var store = provider.GetRequiredService<IAppLoggerStore>();

            // When:
            provider.GetRequiredService<IAppLogger>().Info("message");
            provider.Dispose();

            // Then:
            Assert.That(
                store.GetEntries().Select(entry => entry.Message),
                Has.Some.Contains("log file"));
        }
        finally
        {
            File.Delete(givenBlockedPath);
        }
    }

    [Test]
    public void InitSharedServices__WhenTheJsModuleInteropFactoryIsRegistered__ThenShouldBeScoped()
    {
        // Given:
        // Scoped, because the JavaScript runtime it is built around belongs to a single circuit.
        // A singleton would call into the browser session of whoever resolved it first.
        var givenServices = new ServiceCollection();

        // When:
        givenServices.InitSharedServices();

        // Then:
        ServiceDescriptor result = givenServices.Single(
            descriptor => descriptor.ServiceType == typeof(IJsModuleInteropFactory));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
            Assert.That(result.ImplementationType, Is.EqualTo(typeof(JsModuleInteropFactory)));
        }
    }

    private static ServiceProvider BuildProvider(string logsDirectoryPath)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Path"] = logsDirectoryPath,
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.InitFileLogger());
        services.Configure<FileLoggerOptions>(configuration);
        services.InitAppLogging();

        return services.BuildServiceProvider();
    }
}