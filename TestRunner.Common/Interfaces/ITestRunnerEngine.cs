namespace TestRunner.Common.Interfaces;

using TestRunner.Common.ComplexTypes;

public interface ITestRunnerEngine
{
    bool IsAssemblyLoaded { get; }

    bool IsTestRunning { get; }

    void StopTest(bool force);

    TestSuiteEntity[] GetTestSuiteEntities(string dllPath);
}