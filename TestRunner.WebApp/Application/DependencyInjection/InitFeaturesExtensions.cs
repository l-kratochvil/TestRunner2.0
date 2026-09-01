namespace TestRunner.WebApp.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestRunner.WebApp.Application.Logging;
using TestRunner.WebApp.Features.AppLogging.Services;
using TestRunner.WebApp.Features.TestConfiguration.Services;
using TestRunner.WebApp.Features.TestDiscovery.Services;
using TestRunner.WebApp.Shared.Logging;
using TestRunner.WebApp.Shared.NUnitTestRunner;
using TestRunner.WebApp.Shared.Stores;

/// <summary>
/// Registration of the application services, one method per feature.
/// </summary>
public static class InitFeaturesExtensions
{
    public static IServiceCollection InitFeatures(this IServiceCollection services)
        => services
            .InitAppLogging()
            .InitTestDiscovery()
            .InitTestConfiguration();

    private static IServiceCollection InitAppLogging(this IServiceCollection services)
        => services
            .AddSingleton<IAppLoggerSink, DiagnosticsLoggerSink>()
            .AddSingleton<IAppLoggerFactory, AppLoggerFactory>()
            .AddSingleton(static provider =>
                provider.GetRequiredService<IAppLoggerFactory>()
                        .CreateLogger(LogSources.App))
            .AddSingleton<IAppLoggerStore>(
                static provider =>
                {
                    var store = new AppLoggerStore(provider.GetServices<IAppLoggerSink>());

                    // The log file sits at the far end of the pipeline the log itself feeds, so its
                    // failures cannot travel back as ordinary entries. This is the one wire that carries
                    // them, and it ends in the buffer alone.
                    foreach (var fileLoggerProvider in provider
                        .GetServices<ILoggerProvider>()
                        .OfType<IExtendedLoggerProvider>())
                    {
                        fileLoggerProvider.Failed += store.ReportFailure;
                    }

                    return store;
                });

    private static IServiceCollection InitTestDiscovery(this IServiceCollection services)
        => services.AddScoped<ITestDiscoveryStore, TestDiscoveryStore>();

    private static IServiceCollection InitTestConfiguration(this IServiceCollection services)
        => services.AddScoped<ITestConfigurationStore, TestConfigurationStore>();
}