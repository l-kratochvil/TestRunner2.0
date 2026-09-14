namespace TestRunner.NUnitTestRunnerProxy.Tests;

using System.Reflection;

using DevKit.Core.Extensions.Types;

using NUnit.Framework;

using TestRunner.Common.Model;

// TODO: Test RunTestAsync, etc.
public class NUnitTestRunnerProxyTests
{
    private static readonly string NUnitTestAssembliesDirPath = Path.Combine(
        Assembly.GetExecutingAssembly().GetAssemblyDirectoryPath(),
        "NUnitTestAssemblies");

    private static readonly string TestAssemblyNet461DllPath = Path.Combine(
        NUnitTestAssembliesDirPath,
        "NUnitTestAssembly.Net461",
        "NUnitTestAssembly.Net461.dll");

    private static readonly string TestAssemblyNet481DllPath = Path.Combine(
        NUnitTestAssembliesDirPath,
        "NUnitTestAssembly.Net481",
        "NUnitTestAssembly.Net481.dll");

    [Test]
    public async Task LoadTestAssemblyAsync_WithNet481Assembly()
    {
        var testAssemblyDllPath = TestAssemblyNet481DllPath;
        if (!File.Exists(testAssemblyDllPath))
        {
            throw new FileNotFoundException(testAssemblyDllPath);
        }

        // Given
        var unit = new NUnitTestRunnerProxy();

        // When
        var result = await unit.LoadTestAssemblyAsync(testAssemblyDllPath);

        // Then
        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result, Has.One.Matches<TestSuiteEntity>(x => x.Name == "Net481"));

        var testSuite = result[0];
        Assert.That(testSuite.TestFixtures, Has.One.Matches<TestFixtureEntity>(x => x.Name == "SampleTestSuite"));

        var testFixture = testSuite.TestFixtures[0];
        Assert.That(testFixture.TestCases, Has.Length.EqualTo(4));
        Assert.That(testFixture.TestCases, Has.One.Matches<TestCaseEntity>(x => x.Name == "Pass"));
        Assert.That(testFixture.TestCases, Has.One.Matches<TestCaseEntity>(x => x.Name == "Fail"));
        Assert.That(testFixture.TestCases, Has.One.Matches<TestCaseEntity>(x => x.Name == "Error"));
        Assert.That(testFixture.TestCases, Has.One.Matches<TestCaseEntity>(x => x.Name == "Ignored"));
    }

    [Test]
    public async Task LoadTestAssemblyAsync_WithNet461Assembly()
    {
        var testAssemblyDllPath = TestAssemblyNet461DllPath;
        if (!File.Exists(testAssemblyDllPath))
        {
            throw new FileNotFoundException(testAssemblyDllPath);
        }

        // Given
        var unit = new NUnitTestRunnerProxy();

        // When
        var result = await unit.LoadTestAssemblyAsync(testAssemblyDllPath);

        // Then
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public async Task LoadTestAssemblyAsync_WithZatTestsAssembly()
    {
        var zatTestsAssemblyPath = Path.Combine(@"C:\Automized tests\Test libs\", "Z2xxTests.dll");

        if (!File.Exists(zatTestsAssemblyPath))
        {
            throw new FileNotFoundException(zatTestsAssemblyPath);
        }

        // Given
        var unit = new NUnitTestRunnerProxy();

        // When
        var result = await unit.LoadTestAssemblyAsync(zatTestsAssemblyPath);

        // Then
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public async Task RunTestAsync()
    {
        var testAssemblyDllPath = TestAssemblyNet481DllPath;
        if (!File.Exists(testAssemblyDllPath))
        {
            throw new FileNotFoundException(testAssemblyDllPath);
        }

        // Given
        var unit = new NUnitTestRunnerProxy();

        // When
        var loaded = await unit.LoadTestAssemblyAsync(testAssemblyDllPath);
        var testCaseEntites = loaded
            .SelectMany(x => x.TestFixtures)
            .SelectMany(x => x.TestCases)
            .ToArray();
        var result = await unit.RunTestAsync(testCaseEntites);

        // Then
        Assert.That(result.Status, Is.EqualTo(TestStatus.Failed));
        Assert.That(result.ErrorResults, Has.Length.EqualTo(1));
        Assert.That(result.FailureResults, Has.Length.EqualTo(1));
        Assert.That(result.IgnoredResults, Has.Length.EqualTo(1));
        Assert.That(result.Summary.Errors, Is.EqualTo(1));
        Assert.That(result.Summary.Failures, Is.EqualTo(1));
        Assert.That(result.Summary.Ignored, Is.EqualTo(1));
        Assert.That(result.Summary.Passed, Is.EqualTo(1));
        Assert.That(result.Summary.Total, Is.EqualTo(4));
    }
}