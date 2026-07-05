namespace TestRunner.Common.Model;

// TODO: Rnm TestCase
public class TestCaseEntity(
    TestType testType,
    string name,
    string executionPath)
    : TestEntity(
        testType: testType,
        name: name,
        executionPath: executionPath);