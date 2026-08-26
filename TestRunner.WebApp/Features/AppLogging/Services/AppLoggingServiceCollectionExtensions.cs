namespace TestRunner.WebApp.Features.AppLogging.Services;

using Microsoft.Extensions.DependencyInjection;
using TestRunner.WebApp.Shared.Logging;

/// <summary>
/// Registration of the AppLogging feature.
/// </summary>
public static class AppLoggingServiceCollectionExtensions
{
    /// <summary>
    /// Registers the application log: the shared in-memory store, the daily log file and the
    /// loggers writing into them.
    /// </summary>
    /// <param name="services">Service collection to register into.</param>
    /// <param name="options">Configuration of the application log; defaults are used when omitted.</param>
    /// <returns>The service collection, to allow chaining.</returns>
    public static IServiceCollection AddAppLogging(this IServiceCollection services, AppLoggingOptions? options = null)
    {
        services.AddSingleton(options ?? new AppLoggingOptions());

        services.AddSingleton<AppLogFileSink>();
        services.AddSingleton<IAppLogSink>(provider => provider.GetRequiredService<AppLogFileSink>());
        services.AddHostedService(provider => provider.GetRequiredService<AppLogFileSink>());

        services.AddSingleton<IAppLogStore, AppLogStore>();
        services.AddSingleton<IAppLoggerFactory, AppLoggerFactory>();

        // Convenience registration for the many places that log under the "App" source; anything
        // logging into another channel asks the factory instead.
        services.AddSingleton<IAppLogger>(provider =>
            provider.GetRequiredService<IAppLoggerFactory>().CreateLogger(LogSources.App));

        // Registered last so that it starts after the file sink is ready to accept entries.
        services.AddHostedService<AppLifecycleLogger>();

        return services;
    }
}