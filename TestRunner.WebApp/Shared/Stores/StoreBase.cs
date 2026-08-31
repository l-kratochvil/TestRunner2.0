namespace TestRunner.WebApp.Shared.Stores;

/// <summary>
/// A holder of state that outlives the component reading it: components ask it what the state is,
/// change it through <see cref="UpdateAsync"/> and learn about changes from <see cref="Changed"/>.
/// </summary>
/// <remarks>
/// Remembering the state between visits is deliberately not part of this: a store that needs it
/// takes a <see cref="LocalStorageBase{TData}"/> of its own and writes through on update, so that
/// stores which have nothing worth remembering carry none of the machinery.
/// </remarks>
/// <typeparam name="TState">State the store hands out.</typeparam>
public abstract class StoreBase<TState>
    where TState : class
{
    private TState? current;

    /// <summary>
    /// Raised after the state has changed, on the thread of the caller that changed it.
    /// </summary>
    public event Action? Changed;

    /// <summary>
    /// Gets the state as it stands now.
    /// </summary>
    /// <remarks>
    /// Built on first use rather than in the constructor, so that a derived store may answer with
    /// whatever it is made of without anything being read before it exists.
    /// </remarks>
    public TState Current
        => this.current ??= this.DefaultState;

    /// <summary>
    /// Gets the state the store holds until something changes it.
    /// </summary>
    protected abstract TState DefaultState { get; }

    /// <summary>
    /// Replaces the state with the result of <paramref name="update"/>.
    /// </summary>
    /// <param name="update">Produces the new state from the current one.</param>
    /// <returns>A task that completes once the change has been carried out in full.</returns>
    public virtual Task UpdateAsync(Func<TState, TState> update)
    {
        this.SetState(update(this.Current));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Replaces the state and tells everyone listening, without anything else happening.
    /// </summary>
    /// <remarks>
    /// This is how a derived store changes the state when it must not write through, which is what
    /// restoring what was remembered needs: writing back what was just read would be a change
    /// nobody made.
    /// </remarks>
    /// <param name="state">State to hold from now on.</param>
    protected void SetState(TState state)
    {
        this.current = state;
        this.Changed?.Invoke();
    }
}