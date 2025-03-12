namespace TestRunner.App.Models;

using Microsoft.Extensions.DependencyInjection;

using NUnit;

using TestRunner.App.Services;
using TestRunner.Common;

/// <summary>
/// Test assembly model.
/// </summary>
internal class TestAssembly : ITestAssembly
{
    private TestAssembly(string dllPath)
    {
        var testRunnerEngine = Application.Services.GetRequiredService<ITestRunnerEngine>();

        var settings = new Dictionary<string, object>()
        {
            { FrameworkPackageSettings.WorkDirectory, dllPath }
        };

        this.TestSuites = testRunnerEngine.GetTestEntities(dllPath);
    }

    public TestSuiteEntity[] TestSuites { get; }

    public static TestAssembly Load(string dllPath)
    {
        return new TestAssembly(dllPath);
    }
}