namespace TestRunner.App.Services;

using System;
using System.Runtime.InteropServices;

using TestRunner.App.COM;
using TestRunner.Common;
using TestRunner.Common.ComplexTypes;

/// <summary>
/// Services serving as a fascade for NUnitTestAssemblyRunner.
/// </summary>
internal class TestRunnerEngine : ITestRunnerEngine, IDisposable
{
    // ReSharper disable once SuspiciousTypeConversion.Global
    private readonly INUnitTestRunnerComClient testRunnerComClient = (INUnitTestRunnerComClient)new NUnitTestRunnerComClient();

    public bool IsAssemblyLoaded => testRunnerComClient.IsAssemblyLoaded;

    public bool IsTestRunning => testRunnerComClient.IsTestRunning;

    public async Task<SimpleTypes.TestResult[]> RunTestAsync(IEnumerable<TestSuiteEntity> testsuites, string dllPath)
        => await Task.Run(() => testRunnerComClient.RunTest(testsuites, dllPath));

    public void StopTest(bool force)
        => testRunnerComClient.StopTest(force);

    public TestSuiteEntity[] GetTestSuiteEntities(string dllPath)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
        => Marshal.ReleaseComObject(this.testRunnerComClient);
}