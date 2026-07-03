namespace TestRunner.App.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TestRunner.App.Stores;

internal static class InitStoresExtensions
{
    extension(IHostBuilder hostBuilder)
    {
        public IHostBuilder InitStores()
            => hostBuilder.ConfigureServices(services => services
                .AddSingleton(_ => AppUserSettingsStore.Create())
                .AddSingleton(_ => AppStateStore.Create())
                .AddSingleton<TestRunConfigStore>());
    }
}