namespace TestRunner.NUnitTestRunnerProxy;

using NUnit;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TestRunner.Common.COM;
using TestRunner.Common.ComplexTypes;

using TestResult = Common.TestResult;
using TestStatus = Common.ComplexTypes.TestStatus;

/// <summary>
/// Implementation of <see cref="INUnitTestRunnerProxy"/> COM interface.
/// </summary>
[Guid(Guids.NUnitTestRunnerProxyClassGuid)] // REMINDER: This GUID has to be used in the COM server manifest file
[ComVisible(true)]
public class NUnitTestRunnerProxy : INUnitTestRunnerProxy
{
    // NOTE: The working solution is in the TestRunnerUI project

    public bool IsAssemblyLoaded => runner.IsTestLoaded;

    public bool IsTestRunning => runner.IsTestRunning;

    private readonly ITestAssemblyRunner runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());

    // TEST
    public TestAssemblyEntity LoadTestAssembly(string path)
    {
        var testAssesmblyElement = runner.Load(path, new Dictionary<string, object>());

        if (!testAssesmblyElement.Tests.Any())
        {
            return TestAssemblyEntity.Default;
        }

        var rootTestSuiteElement = testAssesmblyElement.Tests[0]; // Root test element = Z200Tests namespace
        return TransformITestToTestEntities(rootTestSuiteElement);
    }

    // TEST: Mít knihovnu testů s test případy s výsledky: selže, projede, blokovaný
    // TODO: Dořešit filtrování testů 
    public async Task<TestResult> RunTestAsync(IEnumerable<TestAssemblyEntity> testEntitiesToRun)
    {
        // TODO: Ověřit, co se stane, když by tento guard nebyl aktivní a nebyla načtena knihovna
        if (!runner.IsTestLoaded)
        {
            throw new Exception("Test assembly wasn't loaded yet");
        }

        TNode testFilterNode = new("filter");
        FillFilterNodeWithTestEntities(testFilterNode, testEntitiesToRun);
        var testFilter = TestFilter.FromXml(testFilterNode);

        return await Task.Run(() =>
        {
            var result = runner.Run(TestListener.NULL, testFilter);
            return new TestResult(
                result.ResultState.Status switch
                {
                    NUnit.Framework.Interfaces.TestStatus.Passed => TestStatus.Passed,
                    NUnit.Framework.Interfaces.TestStatus.Failed => TestStatus.Failed,
                    NUnit.Framework.Interfaces.TestStatus.Skipped => TestStatus.Skipped,
                    NUnit.Framework.Interfaces.TestStatus.Inconclusive => TestStatus.Inconclusive,
                    NUnit.Framework.Interfaces.TestStatus.Warning => TestStatus.Warning,
                    _ => TestStatus.Unknown
                });
        });
    }

    // TEST: Pro parametr bude použit Stub
    public static TestAssemblyEntity TransformITestToTestEntities(ITest test)
    {
        const string parameterizedMethodTypeName = "parameterizedmethod";

        var children = test.Tests
            .Where(test => Regex.IsMatch(test.TestType.ToLower(), $"testsuite|testfixture|{parameterizedMethodTypeName}"))
            .Select(test => test.TestType.ToLower().Equals(parameterizedMethodTypeName)
                ? new TestAssemblyEntity(DetermineTestType(test), test.Tests[0].Name, test.Tests[0].FullName)
                : TransformITestToTestEntities(test))
            .ToArray();

        return new TestAssemblyEntity(DetermineTestType(test), test.Name, test.FullName, children);

        static TestType DetermineTestType(ITest test) => test.FullName.ToLower().Contains("runtimetests") ? TestType.Runtime : TestType.Common;
    }

    private static void FillFilterNodeWithTestEntities(TNode filterNode, IEnumerable<TestAssemblyEntity> testEntities)
    {
        var testEntitiesInArray = testEntities.ToArray();
        if (!testEntitiesInArray.Any())
        {
            return;
        }

        foreach (var testEntity in testEntitiesInArray)
        {
            filterNode.AddElement("test", testEntity.Path);

            if (testEntity.Children.Any())
            {
                FillFilterNodeWithTestEntities(filterNode, testEntity.Children);
            }
        }
    }

    public void StopTest(bool force = true)
    {
        runner.StopRun(force);
    }

    public ITestSuiteEntity[] GetTestSuiteEntities(string dllPath)
    {
        var settings = new Dictionary<string, object>()
        {
            { FrameworkPackageSettings.WorkDirectory, dllPath }
        };

        // TODO: this.TestSuites = runner.Load(Path.Combine(dllPath), settings)...

        throw new NotImplementedException();
    }
}