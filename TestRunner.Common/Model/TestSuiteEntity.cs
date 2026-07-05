namespace TestRunner.Common.Model;

// TODO: Rnm TestSuite
public class TestSuiteEntity(
    TestCaseEntity[] testcases,
    TestType testType,
    string name,
    string executionPath)
    : TestEntity(
        testType: testType,
        name: name,
        executionPath: executionPath)
{
    public TestCaseEntity[] TestCases { get; } = testcases;
}