namespace Zat.Tests.Runner.WebApp.Shared.TestRunnerBridge;

using Zat.Z2xxTests.Common.Model;
using Zat.Z2xxTests.Common.Services;

public class TestRunnerBridge(
    TestConfig testConfig)
    : ITestRunnerBridge
{
    public Task<TestConfig> GetTestConfigAsync()
        => Task.FromResult(testConfig);
}