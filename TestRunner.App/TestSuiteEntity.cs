namespace TestRunner.App;

using TestRunner.App.Interfaces;

internal class TestSuiteEntity(int id, string name, TestCaseEntity[] testcases)
    : BaseTestEntity(id, name)
{
    private readonly int id = id;

    public TestCaseEntity[] TestCases { get; } = testcases;

    public override TestRunner.App.Interfaces.ITestEntity.TypeKind Type => TestRunner.App.Interfaces.ITestEntity.TypeKind.TestSuite;

    public override int GetHashCode() => CalculateHashCode(id, Name.GetHashCode());

    public override bool Equals(object? obj) => GetHashCode() == obj?.GetHashCode();
}