namespace TestRunner.Common.COM;

using TestRunner.Common.ComplexTypes;
using TestRunner.Common.Interfaces;

/// <summary>
/// Desribes briding interface for COM client-server communication.
/// </summary>
public interface IComBridge : ITestRunnerEngine
{
    SimpleTypes.TestResult[] RunTest(IEnumerable<TestSuiteEntity> testEntitiesToRun, string dllPath);
}