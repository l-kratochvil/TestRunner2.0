namespace TestRunner.Common.Model;

// TODO: Rnm TestSuite
public class TestFixtureEntity(
    TestCaseEntity[] testCases,
    TestType testType,
    string name,
    string executionPath)
    : TestEntity(
        testType: testType,
        name: name,
        executionPath: executionPath)
{
    public TestCaseEntity[] TestCases { get; } = testCases;
}