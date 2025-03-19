namespace TestRunner.App.Services;

using TestRunner.Common;
using TestRunner.Common.ComplexTypes;

internal interface ITestRunnerEngine : TestRunner.Common.Interfaces.ITestRunnerEngine
{
    Task<SimpleTypes.TestResult[]> RunTestAsync(IEnumerable<TestSuiteEntity> testsuites, string dllPath);
}