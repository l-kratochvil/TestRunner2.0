namespace TestRunner.App.Common;

using System.Collections.Generic;

internal abstract class BaseComparer<T> : IEqualityComparer<T>
{
    public delegate bool Comparer(T x, T y);

    protected abstract Comparer ConcreteComparer { get; }

    /// <inheritdoc/>
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

        return this.ConcreteComparer(x, y);
    }

    /// <inheritdoc/>
    public int GetHashCode(T obj) => obj?.GetHashCode() ?? -1;
}