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
            .InitNUnitTestRunner()
            .InitTestDiscovery()
            .InitTestConfiguration();

    /// <summary>
    /// Registers the application log: the shared in-memory store, the sinks mirroring it into the
    /// logging pipeline, and the loggers writing into it.
    /// </summary>
    /// <remarks>
    /// The log reaches the disk only through the logging pipeline, so the log file itself is
    /// registered separately with <see cref="InitLoggingExtensions.InitFileLogger"/>.
    /// </remarks>
    /// <param name="services">Service collection to register into.</param>
    /// <returns>The service collection, to allow chaining.</returns>
    internal static IServiceCollection InitAppLogging(this IServiceCollection services)
        => services
            .AddSingleton<IAppLogSink, DiagnosticsLoggerSink>()
            .AddSingleton<IAppLoggerFactory, AppLoggerFactory>()
            .AddSingleton(static provider =>
                provider.GetRequiredService<IAppLoggerFactory>()
                        .CreateLogger(LogSources.App))
            .AddSingleton<IAppLogStore>(
                static provider =>
                {
                    var store = new AppLogStore(provider.GetServices<IAppLogSink>());

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

    /// <summary>
    /// Registers the test tree read from the test assemblies on start.
    /// </summary>
    /// <remarks>
    /// One instance in three roles. Registering the store and the hosted service separately would
    /// build two of them, and the one filled in on start would not be the one the components read
    /// — a mistake that shows up as an empty test tree and as nothing else.
    /// </remarks>
    /// <param name="services">Service collection to register into.</param>
    /// <returns>The service collection, to allow chaining.</returns>
    internal static IServiceCollection InitNUnitTestRunner(this IServiceCollection services)
        => services
            .AddSingleton<NUnitTestRunnerStore>()
            .AddSingleton<INUnitTestRunnerStore>(
                static provider => provider.GetRequiredService<NUnitTestRunnerStore>())
            .AddHostedService(static provider => provider.GetRequiredService<NUnitTestRunnerStore>());

    /// <summary>
    /// Registers the test selection: the store the feature selects through and the browser storage
    /// remembering what was selected.
    /// </summary>
    /// <remarks>
    /// Selecting belongs to the browser tab it happens in, so both are scoped to the circuit.
    /// Only <see cref="ITestDiscoveryStore"/> is offered to other features, which reads and cannot
    /// select.
    /// </remarks>
    /// <param name="services">Service collection to register into.</param>
    /// <returns>The service collection, to allow chaining.</returns>
    internal static IServiceCollection InitTestDiscovery(this IServiceCollection services)
        => services
            .AddScoped<TestDiscoveryLocalStorage>()
            .AddScoped<TestDiscoveryStore>()
            .AddScoped<ITestDiscoveryStore>(
                static provider => provider.GetRequiredService<TestDiscoveryStore>());

    /// <summary>
    /// Registers the test run configuration and the browser storage remembering it.
    /// </summary>
    /// <param name="services">Service collection to register into.</param>
    /// <returns>The service collection, to allow chaining.</returns>
    internal static IServiceCollection InitTestConfiguration(this IServiceCollection services)
        => services
            .AddScoped<TestConfigurationLocalStorage>()
            .AddScoped<TestConfigurationStore>()
            .AddScoped<ITestConfigurationStore>(
                static provider => provider.GetRequiredService<TestConfigurationStore>());
}