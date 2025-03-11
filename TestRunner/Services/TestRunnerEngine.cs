using System;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace TestRunner.Services;

/// <summary>
/// Services serving as a fascade for NUnitTestAssemblyRunner.
/// </summary>
internal class TestRunnerEngine : ITestRunnerEngine
{
    public bool IsAssemblyLoaded => runner.IsTestLoaded;

    public bool IsTestRunning => runner.IsTestRunning;

    private readonly NUnitTestAssemblyRunner runner = new (new DefaultTestAssemblyBuilder());

    public async Task<Types.TestResult> RunTestAsync(IEnumerable<TestSuiteEntity> testsuites, string dllPath)
    {
        throw new NotImplementedException("TODO: See TestRunnerUI");
    }

    public void StopTest(bool force)
    {
        throw new NotImplementedException();
    }
}
