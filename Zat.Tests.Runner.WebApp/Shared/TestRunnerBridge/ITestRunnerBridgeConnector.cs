namespace Zat.Tests.Runner.WebApp.Shared.TestRunnerBridge;

using Zat.Z2xxTests.Common.Model;

public interface ITestRunnerBridgeConnector
{
    public Task ConnectAsync(
        TestConfig testConfig,
        CancellationToken cancellationToken = default);
}