namespace TestRunner.App;

using TestRunner.App.Interfaces;

internal class TestCaseEntity(int id, string name)
    : BaseTestEntity(id, name)
{
    private readonly int _id = id;

    public override TestRunner.App.Interfaces.ITestEntity.TypeKind Type => TestRunner.App.Interfaces.ITestEntity.TypeKind.TestCase;

    public override bool Equals(object? obj) => GetHashCode() == obj?.GetHashCode();

    public override int GetHashCode() => CalculateHashCode(_id, Name.GetHashCode());
}