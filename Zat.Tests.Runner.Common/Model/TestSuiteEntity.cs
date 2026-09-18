namespace Zat.Tests.Runner.Common.Model;

// TODO: Rnm TestSuite
public class TestSuiteEntity(
    TestFixtureEntity[] testFixtures,
    TestType testType,
    string name,
    string executionPath)
    : TestEntity(
        testType: testType,
        name: name,
        executionPath: executionPath)
{
    public TestFixtureEntity[] TestFixtures { get; } = testFixtures;
}