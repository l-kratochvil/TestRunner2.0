namespace TestRunner.Common.ComplexTypes;

using TestRunner.Common.Interfaces;

public class TestSuiteEntity(int id, string name, TestCaseEntity[] testcases)
    : BaseTestEntity(id, name)
{
    private readonly int id = id;

    public TestCaseEntity[] TestCases { get; } = testcases;

    /// <inheritdoc/>
    public override ITestEntity.TypeKind Type => ITestEntity.TypeKind.TestSuite;

    /// <inheritdoc/>
    public override int GetHashCode() => CommonUtils.CalculateHashCode(this.id, this.Name.GetHashCode());

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.GetHashCode() == obj?.GetHashCode();
}