using System;

namespace TestRunner.App.Services;

internal interface ITestRunnerEngine
{
    bool IsAssemblyLoaded { get; }

    bool IsTestRunning { get; }

    Task<Types.TestResult> RunTestAsync(IEnumerable<TestSuiteEntity> testEntitiesToRun, string dllPath);

    void StopTest(bool force);
}