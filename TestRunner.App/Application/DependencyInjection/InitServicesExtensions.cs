namespace TestRunner.App.Application.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TestRunner.App.TestLinkApi;
using TestRunner.Common.Services;

internal static class InitServicesExtensions
{
    extension(IHostBuilder hostBuilder)
    {
        public IHostBuilder InitServices(
            INUnitTestRunnerProxy nunitTestRunnerProxy)
            => hostBuilder.ConfigureServices(
                services => services
                    .AddSingleton(AppSystemConfig.CreateDefault())
                    .AddSingleton<ITestLinkApiClient, TestLinkApiClient>()
                    .AddSingleton(nunitTestRunnerProxy));
    }
}