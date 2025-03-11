namespace TestRunner;

using TestRunner.Interfaces;

internal abstract class BaseTestEntity(int id, string name) : ITestEntity
{
    public int ID { get; } = id;

    public string Name { get; } = name;

    public abstract ITestEntity.TypeKind Type { get; }
}