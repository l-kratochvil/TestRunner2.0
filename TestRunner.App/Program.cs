using Microsoft.Extensions.Hosting;

using TestRunner.App;
using TestRunner.App.Application.DependencyInjection;

await using var nunitTestRunnerProxyConnector = await NUnitTestRunnerProxyConnector.ConnectAsync();

var host = Host
    .CreateDefaultBuilder()
    .InitServices(nunitTestRunnerProxyConnector.Proxy)
    .InitScreens()
    .InitStores()
    .UseDefaultServiceProvider(
        (_, options) =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        })
    .Build();

await App.RunAsync(host);