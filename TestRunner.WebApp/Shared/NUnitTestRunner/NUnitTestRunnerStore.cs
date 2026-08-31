namespace TestRunner.WebApp.Shared.NUnitTestRunner;

using Microsoft.Extensions.Hosting;
using TestRunner.Common.Model;
using TestRunner.Common.Services;
using TestRunner.WebApp.Shared.Logging;

/// <summary>
/// Reads the test tree from the test assemblies once, when the application starts, and holds it
/// for everyone who shows or runs tests.
/// </summary>
/// <remarks>
/// Discovery is part of starting up rather than of serving a request: the web server begins
/// listening only after every hosted service has started, so by the time the first browser
/// connects, <see cref="LoadedTestSuites"/> is already answered and nothing has to wait for it.
/// </remarks>
/// <param name="proxy">Runner the test assembly is read through.</param>
/// <param name="loggerFactory">Creates the log a discovery failure is reported to.</param>
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
    /// Reads the test assembly and keeps what was discovered.
    /// </summary>
    /// <remarks>
    /// Never fails. A discovery that did not work leaves the application running with no tests to
    /// show, which the user is told about and can act on; letting it throw would take down the
    /// whole application, including the log that explains why.
    /// </remarks>
    /// <param name="cancellationToken">Token abandoning the start.</param>
    /// <returns>A task that completes once discovery has been attempted.</returns>
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