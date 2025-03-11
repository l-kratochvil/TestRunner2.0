namespace TestRunner.App;

using TestRunner.App.Interfaces;

internal abstract class BaseTestEntity(int id, string name) : TestRunner.App.Interfaces.ITestEntity
{
    public int ID { get; } = id;

    public string Name { get; } = name;

    public abstract TestRunner.App.Interfaces.ITestEntity.TypeKind Type { get; }
}