namespace TestRunner.App.Services;

using System;
using System.Runtime.InteropServices;

using TestRunner.App.COM;
using TestRunner.Common;
using TestRunner.Common.COM;
using TestRunner.Common.ComplexTypes;

/// <summary>
/// Services serving as a fascade for NUnitTestAssemblyRunner.
/// </summary>
internal class NUnitTestRunner(COM.INUnitTestRunnerProxy nunitTestRunnerProxy)
    : INUnitTestRunner, IDisposable
{
    public bool IsAssemblyLoaded => nunitTestRunnerProxy.IsAssemblyLoaded;

    public bool IsTestRunning => nunitTestRunnerProxy.IsTestRunning;

    public async Task<TestResult[]> RunTestAsync(IEnumerable<TestSuiteEntity> testsuites, string dllPath)
    {
        return [];
        // TODO: await Task.Run(() => nunitTestRunnerCom.RunTest(testsuites, dllPath));
    }

    public void StopTest(bool force)
        => nunitTestRunnerProxy.StopTest(force);

    public ITestSuiteEntity[] GetTestSuiteEntities(string dllPath)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
        => Marshal.ReleaseComObject(nunitTestRunnerProxy);
}