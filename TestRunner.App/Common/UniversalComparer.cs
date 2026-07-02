namespace TestRunner.App.Common;

internal class UniversalComparer<T>(UniversalComparer<T>.Comparer comparer) : BaseComparer<T>
{
    /// <inheritdoc/>
    protected override Comparer ConcreteComparer { get; } = comparer;
}