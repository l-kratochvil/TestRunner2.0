namespace Zat.Tests.Runner.WebApp.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using Zat.Tests.Runner.WebApp.Features.AppLogging.Services;
using Zat.Tests.Runner.WebApp.Features.AppSettings.Services;
using Zat.Tests.Runner.WebApp.Features.TestConfiguration.Components;
using Zat.Tests.Runner.WebApp.Features.TestDiscovery.Services;
using Zat.Tests.Runner.WebApp.Shared.Logging;
using Zat.Tests.Runner.WebApp.Shared.Stores.AppSettings;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;

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
                .AddSingleton<IAppLoggerHub, AppLoggerHub>();

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
                .AddHostedService(static provider => provider.GetRequiredService<AppSettingsStore>());

        private IServiceCollection InitTestConfiguration()
            => services
                .AddScoped<TestConfigurationViewModel>();
    }
}