using System.Runtime.InteropServices;

using TestRunner.Common.Interfaces;

namespace TestRunner.Common.COM;

/// <summary>
/// Desribes briding interface for COM client-server communication.
/// </summary>
public interface IComBridge : ITestRunnerEngine
{
    Types.TestResult[] RunTest(IEnumerable<TestSuiteEntity> testEntitiesToRun, string dllPath);
}