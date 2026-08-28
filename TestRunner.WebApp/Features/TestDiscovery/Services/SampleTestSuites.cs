namespace TestRunner.WebApp.Features.TestDiscovery.Services;

using TestRunner.Common.Model;

/// <summary>
/// A hand-written test tree standing in for discovery, so the explorer has something to show
/// until the test assemblies are read. Delete this once discovery fills the store.
/// </summary>
public static class SampleTestSuites
{
    /// <summary>
    /// Builds the sample test suites.
    /// </summary>
    /// <returns>The sample test suites.</returns>
    public static TestSuiteEntity[] Create()
        =>
        [
            CreateTestSuite(
                name: "Testy Pertinax6",
                executionPath: "Pertinax6Tests",
                testType: TestType.ApplicationTest,
                fixtureNames: ["Konfigurátor", "Vizualizace"]),
            CreateTestSuite(
                name: "Testy Runtime",
                executionPath: "RuntimeTests",
                testType: TestType.RuntimeTest,
                fixtureNames: ["Komunikace"]),
        ];

    private static TestSuiteEntity CreateTestSuite(
        string name,
        string executionPath,
        TestType testType,
        string[] fixtureNames)
        => new(
            [.. fixtureNames.Select(fixtureName => CreateTestFixture(
                name: fixtureName,
                executionPath: $"{executionPath}.{fixtureName}",
                testType: testType))],
            testType,
            name: name,
            executionPath: executionPath);

    private static TestFixtureEntity CreateTestFixture(
        string name,
        string executionPath,
        TestType testType)
        => new(
            [.. Enumerable.Range(1, 3).Select(number => CreateTestCase(
                name: $"{name} – případ {number}",
                executionPath: $"{executionPath}.Case{number}",
                testType: testType))],
            testType,
            name: name,
            executionPath: executionPath);

    private static TestCaseEntity CreateTestCase(string name, string executionPath, TestType testType)
        => new(
            testType,
            id: executionPath,
            name: name,
            executionPath: executionPath);
}