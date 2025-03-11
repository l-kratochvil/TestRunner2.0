using TestRunner.Interfaces;

namespace TestRunner.Common;

internal class TestEntitiesComparer : BaseComparer<ITestEntity>
{
    protected override Comparer ConcreteComparer { get; } = (x, y) => x.Type.Equals(y.Type) && x.ID.Equals(y.ID);
}