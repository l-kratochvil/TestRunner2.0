namespace TestRunner.NUnitTestRunnerProxy;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit;
using NUnit.Engine;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

using TestRunner.Common.Model;
using TestRunner.Common.Services;

using TestFilter = NUnit.Framework.Internal.TestFilter;
using TestResult = TestRunner.Common.TestResult;
using TestStatus = TestRunner.Common.Model.TestStatus;

/// <summary>
/// Out-of-process NUnit test runner. Hosts the .NET Framework <see cref="ITestAssemblyRunner"/>
/// and is exposed to the application over StreamJsonRpc.
/// </summary>
public sealed class NUnitTestRunnerProxy : INUnitTestRunnerProxy
{
    private readonly NUnitTestAssemblyRunner runner = new(new DefaultTestAssemblyBuilder());

    /// <inheritdoc/>
    public Task<bool> GetIsAssemblyLoadedAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(this.runner.IsTestLoaded);

    /// <inheritdoc/>
    public Task<bool> GetIsTestRunningAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(this.runner.IsTestRunning);

    /// <inheritdoc/>
    public Task<TestSuiteEntity[]> LoadTestAssemblyAsync(
        string path, CancellationToken cancellationToken = default)
        => Task.Run<TestSuiteEntity[]>(
            () =>
            {
                // USING ENGINE
                var testEngine = TestEngineActivator.CreateInstance();
                var package = new TestPackage(path);

                // Run in-process: the NUnit.Engine package does not ship/copy an
                // out-of-process agent (nunit-agent.exe) into this host's output,
                // so the default ProcessRunner fails with a Win32Exception
                // ("cannot find the file") when it tries to launch an agent.
                package.AddSetting("ProcessModel", "InProcess");
                package.AddSetting("DomainUsage", "Single");

                var runner = testEngine.GetRunner(package);
                runner.Load();
                var explored = runner.Explore(NUnit.Engine.TestFilter.Empty);

                // TODO:
                // Tests property is allways empty! Why? Maybe the nunit version doesn't match ...
                // Also look into TestRunner3.0 app logic
                var testAssemblyDirPath = System.IO.Path.GetDirectoryName(path);
                var testAssemblyElement = this.runner.Load(path, new Dictionary<string, object>
                {
                    { FrameworkPackageSettings.WorkDirectory, testAssemblyDirPath },
                    { "ProcessModel", "InProcess" },
                    { "DomainUsage", "Single" },
                });

                var temp = this.runner.ExploreTests(TestFilter.Empty);
                if (!testAssemblyElement.Tests.Any())
                {
                    return [];
                }

                var rootTestSuiteElement = testAssemblyElement.Tests[0]; // Root test element = namespace

                // TODO: Transform to test suites

                return
                [
                    new TestSuiteEntity(
                        [
                            new TestCaseEntity(TestType.RuntimeTest, "Z200_170", "Path"),
                            new TestCaseEntity(TestType.RuntimeTest, "Z200_171", "Path"),
                            new TestCaseEntity(TestType.RuntimeTest, "Z200_180", "Path"),
                            new TestCaseEntity(TestType.RuntimeTest, "Z200_90", "Path"),
                            new TestCaseEntity(TestType.RuntimeTest, "Z200_87", "Path"),
                        ],
                        TestType.ApplicationTest,
                        "TESTSUITE - A",
                        "Path1"),
                    new TestSuiteEntity([], TestType.ApplicationTest, "TESTSUITE - B", "Path2"),
                    new TestSuiteEntity([], TestType.ApplicationTest, "TESTSUITE - C", "Path3"),
                    new TestSuiteEntity([], TestType.ApplicationTest, "TESTSUITE - D", "Path4"),
                    new TestSuiteEntity([], TestType.ApplicationTest, "TESTSUITE - E", "Path5"),
                ];
            },
            cancellationToken);

    /// <inheritdoc/>
    public Task<TestResult> RunTestAsync(
        IEnumerable<TestEntity> testsToRun, CancellationToken cancellationToken = default)
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

    public Task<TestSuiteEntity[]> _LoadTestAssemblyAsync(string path, CancellationToken cancellationToken = default)
        => Task.Run<TestSuiteEntity[]>(
            () =>
            {
                // TODO:
                // Tests property is allways empty! Why? Maybe the nunit version doesn't match ...
                // Also look into TestRunner3.0 app logic 
                var testAssemblyElement = this.runner.Load(path, new Dictionary<string, object>());
                if (!testAssemblyElement.Tests.Any())
                {
                    return [];
                }

                var rootTestSuiteElement = testAssemblyElement.Tests[0]; // Root test element = namespace
                return []; // TODO: Transform to test suites
            },
            cancellationToken);
}