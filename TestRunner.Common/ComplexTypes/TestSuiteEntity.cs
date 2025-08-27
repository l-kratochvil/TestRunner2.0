namespace TestRunner.Common.ComplexTypes;

using Interfaces;

using TestRunner.Common.COM;

public class TestSuiteEntity(int id, string name, TestCaseEntity[] testcases)
    : BaseTestEntity(id, name), ITestSuiteEntity
{
    private readonly int id = id;

    public TestCaseEntity[] TestCases { get; } = testcases;

    public override ITestEntity.TypeKind Type => ITestEntity.TypeKind.TestSuite;

    public override int GetHashCode() => CommonUtils.CalculateHashCode(id, Name.GetHashCode());

    public override bool Equals(object? obj) => GetHashCode() == obj?.GetHashCode();
}