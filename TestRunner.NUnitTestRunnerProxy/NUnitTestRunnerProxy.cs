namespace TestRunner.NUnitTestRunnerProxy;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit;
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
                // Discover tests through the in-process NUnitTestAssemblyRunner.
                // NOTE: this runner calls Assembly.Load and therefore requires the
                // test assembly's bitness to match this host. Runtime test libraries
                // such as Z2xxTests.dll are x86, so this host must run as a 32-bit
                // process (see <PlatformTarget>x86</PlatformTarget> in the proxy/test
                // project); otherwise the assembly is reported as NotRunnable with a
                // BadImageFormatException and no tests are discovered.
                var testAssemblyDirPath = System.IO.Path.GetDirectoryName(path);
                var testAssemblyElement = this.runner.Load(path, new Dictionary<string, object>
                {
                    { FrameworkPackageSettings.WorkDirectory, testAssemblyDirPath },
                });

                if (!testAssemblyElement.Tests.Any())
                {
                    return [];
                }

                var rootTestSuiteElement = testAssemblyElement.Tests[0]; // Root test element = namespace

                return [..CollectTestSuiteEntities(rootTestSuiteElement)];
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

    private static IEnumerable<TestSuiteEntity> CollectTestSuiteEntities(ITest root)
        => root.Tests.OfType<TestSuite>().Select(x =>
        {
            var testType = x.FullName.ToLower().Contains("runtimetests")
                ? TestType.RuntimeTest
                : TestType.ApplicationTest;
            return new TestSuiteEntity(
                [.. CollectTestFixtureEntities(x, testType)],
                testType,
                name: x.Name,
                executionPath: x.FullName);
        });

    private static IEnumerable<TestFixtureEntity> CollectTestFixtureEntities(
        TestSuite testSuite, TestType testType)
        => testSuite
            .Tests
            .OfType<TestFixture>()
            .Select(x => new TestFixtureEntity(
                [..CollectTestCaseEntities(x, testType)],
                testType,
                name: x.Name,
                executionPath: x.FullName));

    private static IEnumerable<TestCaseEntity> CollectTestCaseEntities(
        TestFixture testFixture, TestType testType)
        => testFixture
            .Tests
            .OfType<ParameterizedMethodSuite>()
            .Select(x => new TestCaseEntity(
                testType,
                name: x.Name,
                executionPath: x.FullName));

    private static void FillFilterNodeWithTestEntities(TNode filterNode, IEnumerable<TestEntity> testEntities)
    {
        var testEntitiesInArray = testEntities.ToArray();
        if (!testEntitiesInArray.Any())
        {
            return;
        }

        foreach (var testEntity in testEntitiesInArray)
        {
            filterNode.AddElement("test", testEntity.ExecutionPath);
        }
    }
}