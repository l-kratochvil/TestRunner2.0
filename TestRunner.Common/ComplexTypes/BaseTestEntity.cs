namespace TestRunner.Common.ComplexTypes;

public abstract class BaseTestEntity(int id, string name) : Interfaces.ITestEntity
{
    public int ID { get; } = id;

    public string Name { get; } = name;

    public abstract Interfaces.ITestEntity.TypeKind Type { get; }
}