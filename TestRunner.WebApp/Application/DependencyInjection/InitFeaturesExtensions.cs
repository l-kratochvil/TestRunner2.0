namespace TestRunner.WebApp.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TestRunner.WebApp.Features.AppLogging.Services;
using TestRunner.WebApp.Features.AppSettings.Services;
using TestRunner.WebApp.Features.TestConfiguration.Services;
using TestRunner.WebApp.Features.TestDiscovery.Services;
using TestRunner.WebApp.Shared.Logging;
using TestRunner.WebApp.Shared.Storage;
using TestRunner.WebApp.Shared.Stores;

/// <summary>
/// Registration of the application services, one method per feature.
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
                    provider.GetRequiredService<IAppLoggerFactory>()
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
        /// The settings describe the machine the application runs on, so one instance serves every
        /// circuit, and it is started as a hosted service to have read its file before the first
        /// browser is answered.
        /// </remarks>
        private IServiceCollection InitAppSettings()
            => services
                .AddOptions<AppSettingsOptions>()
                .BindConfiguration("AppSettings")
                .Services
                .AddSingleton(
                    static provider => new JsonFileStorage<AppSettingsState>(
                        provider.GetRequiredService<IOptions<AppSettingsOptions>>().Value.FilePath,
                        provider.GetRequiredService<IAppLoggerFactory>().CreateLogger(LogSources.App),
                        static () => new AppSettingsState(AppSettingsStore.DefaultIdeInstallFolderPath)))
                .AddSingleton<AppSettingsStore>()
                .AddSingleton<IAppSettingsStore>(
                    static provider => provider.GetRequiredService<AppSettingsStore>())
                .AddHostedService(static provider => provider.GetRequiredService<AppSettingsStore>());

        private IServiceCollection InitTestConfiguration()
            => services
                .AddSingleton<IInstalledRuntimeVersionsProvider, InstalledRuntimeVersionsProvider>()
                .AddSingleton<ITestConfigurationValidator, TestConfigurationValidator>();
    }
}
