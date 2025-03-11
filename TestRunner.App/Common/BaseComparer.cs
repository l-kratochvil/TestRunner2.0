namespace TestRunner.App.Common;

internal abstract class BaseComparer<T> : IEqualityComparer<T>
{
    public delegate bool Comparer(T x, T y);

    protected abstract Comparer ConcreteComparer { get; }

    public bool Equals(T? x, T? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        return ConcreteComparer(x, y);
    }

    public int GetHashCode(T obj) => obj?.GetHashCode() ?? -1;
}