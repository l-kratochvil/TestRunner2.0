// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;

using TestRunner.App;
using TestRunner.Common.Services;

await using var connection = await NUnitTestRunnerProxyConnection.StartAsync();

var services = new ServiceCollection();
Application.ConfigureServices(services);
services.AddSingleton(connection.Proxy);
Application.Services = services.BuildServiceProvider();

new Application().Run();