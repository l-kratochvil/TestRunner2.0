namespace TestRunner.WebApp.Application.DependencyInjection;

using Fluxor;
using Fluxor.Persist.Middleware;
using Fluxor.Persist.Storage;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

using TestRunner.WebApp.Shared.JsInterop;
using TestRunner.WebApp.Shared.NUnitTestRunner;
using TestRunner.WebApp.Shared.Storage;
using TestRunner.WebApp.Shared.Stores;

/// <summary>
/// Registration of the application services, one method per feature.
/// </summary>
public static class InitServicesExtension
{
    /// <param name="services">Service collection to register into.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the services shared across features: the ones living under <c>Shared</c> and
        /// belonging to no feature of their own.
        /// </summary>
        /// <remarks>
        /// The JS module wrappers are scoped because the <see cref="IJSRuntime"/> they are built
        /// around is, see <see cref="JsModuleInteropFactory"/>.
        /// </remarks>
        /// <returns>The service collection, to allow chaining.</returns>
        public IServiceCollection InitSharedServices()
            => services
                .AddScoped<IJsModuleInteropFactory, JsModuleInteropFactory>()
                .AddSingleton<BrowserLogger>()
                .AddSingleton<IDirectoryReader, DirectoryReader>()
                .InitFluxor()
                .InitNUnitTestRunner();

        private IServiceCollection InitNUnitTestRunner()
            => services
                .AddSingleton<NUnitTestRunnerStore>()
                .AddSingleton<INUnitTestRunnerStore>(
                    static provider => provider.GetRequiredService<NUnitTestRunnerStore>())
                .AddHostedService(static provider => provider.GetRequiredService<NUnitTestRunnerStore>());

        private IServiceCollection InitFluxor()
            => services
                .AddFluxor(
                    options => options
                        .ScanAssemblies(typeof(Program).Assembly)
                        .UsePersist(options =>
                        {
                            // Only what is listed here is remembered by the browser, and it is
                            // matched against the name of the feature. Fluxor names a feature after
                            // the full name of its state unless the state says otherwise, so the
                            // two only meet because TestConfigurationState names itself.
                            options.UseInclusionApproach();
                            options.SetWhiteList([nameof(TestConfigurationState)]);
                        }))
                .AddScoped<IStringStateStorage, LocalStringStateStorage>()
                .AddScoped<IStoreHandler, JsonStoreHandler>();
    }
}