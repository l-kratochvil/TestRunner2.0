namespace TestRunner.NUnitTestRunnerProxy;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

using TestRunner.Common.Model;
using TestRunner.Common.Services;

using TestResult = TestRunner.Common.TestResult;
using TestStatus = TestRunner.Common.Model.TestStatus;

/// <summary>
/// Out-of-process NUnit test runner. Hosts the .NET Framework <see cref="ITestAssemblyRunner"/>
/// and is exposed to the application over StreamJsonRpc.
/// </summary>
public sealed class NUnitTestRunnerProxy : INUnitTestRunnerProxy
{
    private readonly ITestAssemblyRunner runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());

    /// <inheritdoc/>
    public Task<bool> GetIsAssemblyLoadedAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(this.runner.IsTestLoaded);

    /// <inheritdoc/>
    public Task<bool> GetIsTestRunningAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(this.runner.IsTestRunning);

    /// <inheritdoc/>
    public Task<TestSuiteEntity[]> LoadTestAssemblyAsync(string path, CancellationToken cancellationToken = default)
        => Task.Run<TestSuiteEntity[]>(
            () =>
            {
                var testAssemblyElement = this.runner.Load(path, new Dictionary<string, object>());

                if (!testAssemblyElement.Tests.Any())
                {
                    return [];
                }

                var rootTestSuiteElement = testAssemblyElement.Tests[0]; // Root test element = namespace
                return []; // TODO: Transform to test suites
            },
            cancellationToken);

    /// <inheritdoc/>
    public Task<TestResult> RunTestAsync(IEnumerable<TestEntity> testsToRun, CancellationToken cancellationToken = default)
    {
        if (!this.runner.IsTestLoaded)
        {
            throw new InvalidOperationException("Test assembly wasn't loaded yet");
        }

        var testFilterNode = new TNode("filter");
        FillFilterNodeWithTestEntities(testFilterNode, testsToRun);
        var testFilter = TestFilter.FromXml(testFilterNode);

        // Cancellation forcibly aborts the in-progress run.
        var cancellationRegistration = cancellationToken.Register(() => this.runner.StopRun(force: true));

        return Task.Run(
            () =>
            {
                try
                {
                    var result = this.runner.Run(TestListener.NULL, testFilter);
                    return new TestResult(
                        result.ResultState.Status switch
                        {
                            NUnit.Framework.Interfaces.TestStatus.Passed => TestStatus.Passed,
                            NUnit.Framework.Interfaces.TestStatus.Failed => TestStatus.Failed,
                            NUnit.Framework.Interfaces.TestStatus.Skipped => TestStatus.Skipped,
                            NUnit.Framework.Interfaces.TestStatus.Inconclusive => TestStatus.Inconclusive,
                            NUnit.Framework.Interfaces.TestStatus.Warning => TestStatus.Warning,
                            _ => TestStatus.Unknown,
                        });
                }
                finally
                {
                    cancellationRegistration.Dispose();
                }
            },
            cancellationToken);
    }

    // private static TestEntity TransformITestToTestEntities(ITest test)
    // {
    //     const string parameterizedMethodTypeName = "parameterizedmethod";

    //     var children = test.Tests
    //         .Where(t => Regex.IsMatch(t.TestType.ToLower(), $"testsuite|testfixture|{parameterizedMethodTypeName}"))
    //         .Select(t => t.TestType.ToLower().Equals(parameterizedMethodTypeName)
    //             ? new TestEntity(DetermineTestType(t), t.Tests[0].Name, t.Tests[0].FullName)
    //             : TransformITestToTestEntities(t))
    //         .ToArray();

    //     return new TestEntity(DetermineTestType(test), test.Name, test.FullName, children);

    //     static TestType DetermineTestType(ITest test) => test.FullName.ToLower().Contains("runtimetests") ? TestType.RuntimeTest : TestType.ApplicationTest;
    // }

    private static void FillFilterNodeWithTestEntities(TNode filterNode, IEnumerable<TestEntity> testEntities)
    {
        var testEntitiesInArray = testEntities.ToArray();
        if (!testEntitiesInArray.Any())
        {
            return;
        }

        foreach (var testEntity in testEntitiesInArray)
        {
            filterNode.AddElement("test", testEntity.Path);
        }
    }
}