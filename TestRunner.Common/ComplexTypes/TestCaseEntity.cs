namespace TestRunner.Common.ComplexTypes;

using TestRunner.Common.Interfaces;

public class TestCaseEntity(int id, string name)
    : BaseTestEntity(id, name)
{
    private readonly int _id = id;

    /// <inheritdoc/>
    public override ITestEntity.TypeKind Type => ITestEntity.TypeKind.TestCase;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.GetHashCode() == obj?.GetHashCode();

    /// <inheritdoc/>
    public override int GetHashCode() => CommonUtils.CalculateHashCode(this._id, this.Name.GetHashCode());
}