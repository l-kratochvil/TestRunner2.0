namespace TestRunner.Common.Interfaces;

using System;

using TestRunner.Common;

public interface ITestRunnerEngine
{
    bool IsAssemblyLoaded { get; }

    bool IsTestRunning { get; }

    void StopTest(bool force);

    TestSuiteEntity[] GetTestEntities(string dllPath);
}