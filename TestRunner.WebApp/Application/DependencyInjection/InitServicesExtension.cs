namespace TestRunner.WebApp.Application.DependencyInjection;

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using TestRunner.WebApp.Application.Logging;
using TestRunner.WebApp.Features.AppLogging.Services;
using TestRunner.WebApp.Features.TestDiscovery.Services;
using TestRunner.WebApp.Shared.JsInterop;
using TestRunner.WebApp.Shared.Logging;
using TestRunner.WebApp.Shared.NUnitTestRunner;
using TestRunner.WebApp.Shared.Stores;

/// <summary>
/// Registration of the application services, one method per feature.
/// </summary>
public static class InitServicesExtension
{
    /// <summary>
    /// Registers the services shared across features: the ones living under <c>Shared</c> and
    /// belonging to no feature of their own.
    /// </summary>
    /// <remarks>
    /// The JS module wrappers are scoped because the <see cref="IJSRuntime"/> they are built
    /// around is, see <see cref="JsModuleInteropFactory"/>.
    /// </remarks>
    /// <param name="services">Service collection to register into.</param>
    /// <returns>The service collection, to allow chaining.</returns>
    public static IServiceCollection InitSharedServices(this IServiceCollection services)
        => services
            .AddScoped<IJsModuleInteropFactory, JsModuleInteropFactory>()
            .AddSingleton<BrowserLogger>()
            .InitNUnitTestRunner();

    private static IServiceCollection InitNUnitTestRunner(this IServiceCollection services)
        => services
            .AddSingleton<NUnitTestRunnerStore>()
            .AddSingleton<INUnitTestRunnerStore>(
                static provider => provider.GetRequiredService<NUnitTestRunnerStore>())
            .AddHostedService(static provider => provider.GetRequiredService<NUnitTestRunnerStore>());
}