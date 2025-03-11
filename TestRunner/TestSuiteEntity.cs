namespace TestRunner;

using TestRunner.Interfaces;

internal class TestSuiteEntity(int id, string name, TestCaseEntity[] testcases)
    : BaseTestEntity(id, name)
{
    private readonly int id = id;

    public TestCaseEntity[] TestCases { get; } = testcases;

    public override ITestEntity.TypeKind Type => ITestEntity.TypeKind.TestSuite;

    public override int GetHashCode() => CalculateHashCode(id, Name.GetHashCode());

    public override bool Equals(object? obj) => GetHashCode() == obj?.GetHashCode();
}