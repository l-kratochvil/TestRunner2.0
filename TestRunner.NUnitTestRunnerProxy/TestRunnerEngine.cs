namespace TestRunner.NUnitTestRunnerProxy;

using NUnit;
using NUnit.Framework.Api;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using TestRunner.Common;
using TestRunner.Common.Interfaces;

// FACADE PATTERN PRO NUNIT ENGINE
public class TestRunnerEngine : ITestRunnerEngine
{
    #region IMPLEMENTATION FROM TESTRUNNER UI

    //// TEST
    //public TestRunEntity LoadTestAssembly(string path)
    //{
    //    var testAssesmblyElement = runner.Load(path, new Dictionary<string, object>());

    //    if (!testAssesmblyElement.Tests.Any())
    //    {
    //        return TestRunEntity.Default;
    //    }

    //    var rootTestSuiteElement = testAssesmblyElement.Tests[0]; // Root test element = Z200Tests namespace
    //    return TransformITestToTestEntities(rootTestSuiteElement);
    //}

    //// TEST: Mít knihovnu testů s test případy s výsledky: selže, projede, blokovaný
    //// TODO: Dořešit filtrování testů 
    //public async Task<TestResult> RunTestAsync(IEnumerable<TestRunEntity> testEntitiesToRun)
    //{
    //    // TODO: Ověřit, co se stane, když by tento guard nebyl aktivní a nebyla načtena knihovna
    //    if (!runner.IsTestLoaded)
    //    {
    //        throw new Exception("Test assembly wasn't loaded yet");
    //    }

    //    TNode testFilterNode = new("filter");
    //    FillFilterNodeWithTestEntities(testFilterNode, testEntitiesToRun);
    //    var testFilter = TestFilter.FromXml(testFilterNode);

    //    return await Task.Run(() =>
    //    {
    //        var result = runner.Run(TestListener.NULL, testFilter);
    //        return new TestResult(result.ResultState.Status);
    //    });
    //}

    //public void StopTest(bool force = true)
    //{
    //    runner.StopRun(force);
    //}

    //// TEST: Pro parametr bude použit Stub
    //public static TestRunEntity TransformITestToTestEntities(ITest test)
    //{
    //    const string parameterizedMethodTypeName = "parameterizedmethod";

    //    var children = test.Tests
    //        .Where(test => Regex.IsMatch(test.TestType.ToLower(), $"testsuite|testfixture|{parameterizedMethodTypeName}"))
    //        .Select(test => test.TestType.ToLower().Equals(parameterizedMethodTypeName)
    //            ? new TestRunEntity(DetermineTestType(test), test.Tests[0].Name, test.Tests[0].FullName)
    //            : TransformITestToTestEntities(test))
    //        .ToArray();

    //    return new TestRunEntity(DetermineTestType(test), test.Name, test.FullName, children);

    //    static TestType DetermineTestType(ITest test) => test.FullName.ToLower().Contains("runtimetests") ? TestType.Runtime : TestType.Common;
    //}

    //private static void FillFilterNodeWithTestEntities(TNode filterNode, IEnumerable<TestRunEntity> testEntities)
    //{
    //    if (testEntities is null)
    //    {
    //        return;
    //    }

    //    var testEntitiesInArray = testEntities.ToArray();
    //    if (!testEntitiesInArray.Any())
    //    {
    //        return;
    //    }

    //    foreach (var testEntity in testEntitiesInArray)
    //    {
    //        filterNode.AddElement("test", testEntity.Path);

    //        if (testEntity.Children.Any())
    //        {
    //            FillFilterNodeWithTestEntities(filterNode, testEntity.Children);
    //        }
    //    }
    //}

    #endregion

    public bool IsAssemblyLoaded => runner.IsTestLoaded;

    public bool IsTestRunning => runner.IsTestRunning;

    private readonly ITestAssemblyRunner runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());

    public async Task<Types.TestResult> RunTestAsync(IEnumerable<TestSuiteEntity> testsuites, string dllPath)
    {
        throw new NotImplementedException("TODO: See TestRunnerUI");
    }

    public void StopTest(bool force)
    {
        throw new NotImplementedException();
    }

    public TestSuiteEntity[] GetTestEntities(string dllPath)
    {
        var settings = new Dictionary<string, object>()
        {
            { FrameworkPackageSettings.WorkDirectory, dllPath }
        };

        // TODO: this.TestSuites = runner.Load(Path.Combine(dllPath), settings)...

        throw new NotImplementedException();
    }
}