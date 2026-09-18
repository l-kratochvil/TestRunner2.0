namespace Zat.Tests.Runner.Common.Model;

// TODO: Rnm TestCase
public class TestCaseEntity(
    TestType testType,
    string id,
    string name,
    string executionPath)
    : TestEntity(
        testType: testType,
        name: name,
        executionPath: executionPath)
{
    public string Id { get; set; } = id;
}