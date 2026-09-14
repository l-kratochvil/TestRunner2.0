namespace TestRunner.WebApp.Shared.NUnitTestRunner;

using Microsoft.Extensions.Hosting;
using TestRunner.Common.Model;
using TestRunner.Common.Services;
using TestRunner.WebApp.Shared.Logging;

/// <summary>
/// Store of test suites discovered from the test machine when the application starts.
/// </summary>
/// <remarks>
/// Discovery runs during startup, so <see cref="LoadedTestSuites"/> is ready before the first
/// browser connects.
/// </remarks>
/// <param name="proxy">Proxy the test assembly is discovered through.</param>
/// <param name="loggerFactory">Creates the log discovery failures are reported to.</param>
public sealed class NUnitTestRunnerStore(INUnitTestRunnerProxy proxy, IAppLoggerFactory loggerFactory)
    : INUnitTestRunnerStore, IHostedService
{
    // TODO: The test assembly is picked by the tester, so this belongs to the application settings
    // rather than to the code, see the AppSettings feature.
    private const string TestAssemblyPath =
        @"c:\Users\l-kratochvil\source\repos\TestRunner2.0\Tests\NUnitTestAssembly.Net481\bin\Debug\net481\NUnitTestAssembly.Net481.dll";

    private readonly IAppLogger logger = loggerFactory.CreateLogger(LogSources.TestRun);

    /// <inheritdoc/>
    public IReadOnlyList<TestSuiteEntity> LoadedTestSuites { get; private set; } = [];

    /// <summary>
    /// Discovers test suites from the configured test assembly.
    /// </summary>
    /// <remarks>
    /// Discovery failures do not stop the application. The store stays empty and the log explains
    /// why.
    /// </remarks>
    /// <param name="cancellationToken">Token abandoning the start.</param>
    /// <returns>A task that completes after discovery has been attempted.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            this.LoadedTestSuites = await proxy.LoadTestAssemblyAsync(TestAssemblyPath, cancellationToken);

            this.logger.Info($"Loaded {this.LoadedTestSuites.Count} test suites.", TestAssemblyPath);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            this.logger.Warning(
                "The test assembly could not be read, so there are no tests to choose from.",
                exception.ToString());
        }
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}