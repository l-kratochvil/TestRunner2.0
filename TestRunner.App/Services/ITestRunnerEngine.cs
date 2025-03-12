namespace TestRunner.App.Services;

using TestRunner.Common;

internal interface ITestRunnerEngine : TestRunner.Common.Interfaces.ITestRunnerEngine
{
    Task<Types.TestResult[]> RunTestAsync(IEnumerable<TestSuiteEntity> testsuites, string dllPath);
}