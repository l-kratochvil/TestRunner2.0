using TestRunner.App.Interfaces;

namespace TestRunner.App.Common;

internal class TestEntitiesComparer : BaseComparer<TestRunner.App.Interfaces.ITestEntity>
{
    protected override Comparer ConcreteComparer { get; } = (x, y) => x.Type.Equals(y.Type) && x.ID.Equals(y.ID);
}