namespace TestRunner.App.Common;

using TestRunner.Common.Interfaces;

internal class TestEntitiesComparer : BaseComparer<ITestEntity>
{
    /// <inheritdoc/>
    protected override Comparer ConcreteComparer { get; } = (x, y) => x.Type.Equals(y.Type) && x.ID.Equals(y.ID);
}