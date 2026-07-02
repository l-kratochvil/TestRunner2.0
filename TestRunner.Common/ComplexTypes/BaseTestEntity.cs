namespace TestRunner.Common.ComplexTypes;

public abstract class BaseTestEntity(int id, string name) : Interfaces.ITestEntity
{
    /// <inheritdoc/>
    public int ID { get; } = id;

    /// <inheritdoc/>
    public string Name { get; } = name;

    /// <inheritdoc/>
    public abstract Interfaces.ITestEntity.TypeKind Type { get; }
}