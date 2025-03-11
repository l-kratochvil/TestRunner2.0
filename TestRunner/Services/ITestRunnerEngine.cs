using System;

namespace TestRunner.Services;

internal interface ITestRunnerEngine
{
    bool IsAssemblyLoaded { get; }

    bool IsTestRunning { get; }

    Task<TestResult> RunTestAsync(IEnumerable<TestSuiteEntity> testEntitiesToRun, string dllPath);

    void StopTest(bool force);
}
