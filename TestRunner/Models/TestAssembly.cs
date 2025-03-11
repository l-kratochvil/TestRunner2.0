using System;
using NUnit;
using NUnit.Framework.Api;

namespace TestRunner.Models;

/// <summary>
/// Test assembly model.
/// </summary>
internal class TestAssembly : ITestAssembly
{
    private TestAssembly(string dllPath)
    {
        ITestAssemblyRunner runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());

        var settings = new Dictionary<string, object>()
        {
            { FrameworkPackageSettings.WorkDirectory, dllPath }
        };

        // TODO: this.TestSuites = runner.Load(Path.Combine(dllPath), settings)...
    }

    public TestSuiteEntity[] TestSuites { get; }

    public static TestAssembly Load(string dllPath)
    {
        return new TestAssembly(dllPath);
    }
}
