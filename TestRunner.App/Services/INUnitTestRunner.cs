namespace TestRunner.App.Services;

using TestRunner.Common;
using TestRunner.Common.ComplexTypes;

public interface INUnitTestRunner : TestRunner.App.COM.INUnitTestRunnerProxy
{
    Task<TestResult[]> RunTestAsync(IEnumerable<TestSuiteEntity> testsuites, string dllPath);
}