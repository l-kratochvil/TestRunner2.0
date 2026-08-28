namespace TestRunner.WebApp.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using TestRunner.WebApp.Application.Logging;
using TestRunner.WebApp.Features.AppLogging.Services;
using TestRunner.WebApp.Shared.JsInterop;
using TestRunner.WebApp.Shared.Logging;

/// <summary>
/// Registration of the application services, one method per feature.
/// </summary>
public static class InitServicesExtension
{
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
    public static IServiceCollection InitAppLogging(this IServiceCollection services)
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
    /// Registers the services shared across features: the ones living under <c>Shared</c> and
    /// belonging to no feature of their own.
    /// </summary>
    /// <remarks>
    /// The JS module wrappers are scoped because the <see cref="IJSRuntime"/> they are built
    /// around is, see <see cref="JsModuleInteropFactory"/>. <see cref="BrowserLogger"/> holds no
    /// circuit of its own — only a logger — so one instance serves every browser.
    /// </remarks>
    /// <param name="services">Service collection to register into.</param>
    /// <returns>The service collection, to allow chaining.</returns>
    public static IServiceCollection InitSharedServices(this IServiceCollection services)
        => services
            .AddScoped<IJsModuleInteropFactory, JsModuleInteropFactory>()
            .AddSingleton<BrowserLogger>();
}