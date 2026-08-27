namespace TestRunner.WebApp.Tests.Application.Logging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using TestRunner.WebApp.Application.Logging;

[TestFixture]
public class InitLoggingExtensionsTests
{
    [Test]
    public void InitFileLogger__WhenTheLoggingSectionIsConfigured__ThenShouldBindTheOptions()
    {
        // Given:
        string givenPath = Path.Combine(Path.GetTempPath(), $"filelogger-binding-{Guid.NewGuid():N}");
        IConfiguration givenConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:File:Path"] = givenPath,
                ["Logging:File:RetainedFileCount"] = "3",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddConfiguration(givenConfiguration.GetSection("Logging"));
            builder.InitFileLogger();
        });

        // When:
        using ServiceProvider provider = services.BuildServiceProvider();
        FileLoggerOptions result = provider.GetRequiredService<IOptions<FileLoggerOptions>>().Value;

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(result.Path, Is.EqualTo(givenPath));
            Assert.That(result.RetainedFileCount, Is.EqualTo(3));
        });
    }
}