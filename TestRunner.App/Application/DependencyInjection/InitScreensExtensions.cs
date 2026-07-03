namespace TestRunner.App.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TestRunner.App.Screens;

internal static class InitScreensExtensions
{
    extension(IHostBuilder hostBuilder)
    {
        public IHostBuilder InitScreens()
            => hostBuilder.ConfigureServices(services => services
                .AddSingleton<HomeScreen>()
                .AddSingleton<EmptyScreen>()
                .AddSingleton<ExitScreen>()
                .AddSingleton<SettingsScreen>()
                .AddSingleton<IdeVersionPromptScreen>()
                .AddSingleton<RuntimeVersionPromptScreen>()
                .AddSingleton<TestSuitesSelectionScreen>()
                .AddSingleton<TestCasesSelectionScreen>()
                .AddSingleton(static provider => new Lazy<ExitScreen>(provider.GetRequiredService<ExitScreen>))
                .AddSingleton(static provider => new Lazy<SettingsScreen>(provider.GetRequiredService<SettingsScreen>)));
    }
}