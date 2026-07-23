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
        if (!File.Exists(TestAssemblyNet481DllPath))
        {
            throw new FileNotFoundException(TestAssemblyNet481DllPath);
        }

        // Given
        var unit = new NUnitTestRunnerProxy();

        // When
        var result = await unit.LoadTestAssemblyAsync(TestAssemblyNet481DllPath);

        // Then
        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result, Has.One.Matches<TestSuiteEntity>(x => x.Name == "Net481"));

        var testSuite = result[0];
        Assert.That(testSuite.TestFixtures, Has.One.Matches<TestFixtureEntity>(x => x.Name == "SampleTestSuite"));

        var testFixture = testSuite.TestFixtures[0];
        Assert.That(testFixture.TestCases, Has.Length.EqualTo(1));
        Assert.That(testFixture.TestCases[0].Name, Is.EqualTo("SampleTestCase"));
    }

    [Test]
    public async Task LoadTestAssemblyAsync_WithNet461Assembly()
    {
        if (!File.Exists(TestAssemblyNet461DllPath))
        {
            throw new FileNotFoundException(TestAssemblyNet461DllPath);
        }

        // Given
        var unit = new NUnitTestRunnerProxy();

        // When
        var result = await unit.LoadTestAssemblyAsync(TestAssemblyNet461DllPath);

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
}