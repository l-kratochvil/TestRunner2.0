namespace TestRunner.WebApp.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using TestRunner.WebApp.Features.AppLogging.Services;
using TestRunner.WebApp.Features.AppSettings.Services;
using TestRunner.WebApp.Features.TestConfiguration.Services;
using TestRunner.WebApp.Features.TestDiscovery.Services;
using TestRunner.WebApp.Shared.Logging;
using TestRunner.WebApp.Shared.Stores.AppSettings;
using TestRunner.WebApp.Shared.Stores.TestDiscovery;
using TestRunner.WebApp.Shared.Validation;

/// <summary>
/// Feature-by-feature registration of application services.
/// </summary>
public static class InitFeaturesExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection InitFeatures()
            => services
                .InitAppLogging()
                .InitAppSettings()
                .InitTestDiscovery()
                .InitTestConfiguration();

        private IServiceCollection InitAppLogging()
            => services
                .AddSingleton<IAppLoggerSink, DiagnosticsLoggerSink>()
                .AddSingleton<IAppLoggerFactory, AppLoggerFactory>()
                .AddSingleton(static provider =>
                    provider
                        .GetRequiredService<IAppLoggerFactory>()
                        .CreateLogger(LogSources.App))
                .AddSingleton<IAppLoggerHub>(
                    static provider =>
                    {
                        var loggerHub = new AppLoggerHub(provider.GetServices<IAppLoggerSink>());

                        // The log file sits at the far end of the pipeline the log itself feeds, so its
                        // failures cannot travel back as ordinary entries. This is the one wire that carries
                        // them, and it ends in the buffer alone.
                        foreach (var fileLoggerProvider in provider
                                     .GetServices<ILoggerProvider>()
                                     .OfType<IExtendedLoggerProvider>())
                        {
                            fileLoggerProvider.Failed += loggerHub.ReportFailure;
                        }

                        return loggerHub;
                    });

        private IServiceCollection InitTestDiscovery()
            => services
                .AddScoped<TestDiscoveryStore>()
                .AddScoped<ITestDiscoveryStore>(
                    provider => provider.GetRequiredService<TestDiscoveryStore>());

        /// <remarks>
        /// The settings describe one machine, so one <see cref="AppSettingsStore"/> is shared across
        /// circuits and started before the first browser response.
        /// </remarks>
        private IServiceCollection InitAppSettings()
            => services
                .AddSingleton<AppSettingsStore>()
                .AddSingleton<IAppSettingsStore>(
                    static provider => provider.GetRequiredService<AppSettingsStore>())
                .AddHostedService(static provider => provider.GetRequiredService<AppSettingsStore>())
                .AddSingleton<IAppSettingsValidator, AppSettingsValidator>();

        private IServiceCollection InitTestConfiguration()
            => services
                .AddSingleton<IInstalledRuntimeVersionsProvider, InstalledRuntimeVersionsProvider>()
                .AddSingleton<ITestConfigurationValidator, TestConfigurationValidator>();
    }
}
