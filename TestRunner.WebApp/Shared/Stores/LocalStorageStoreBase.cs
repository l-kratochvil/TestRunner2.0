namespace TestRunner.WebApp.Shared.Stores;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TestRunner.WebApp.Shared.Logging;

/// <summary>
/// A store whose state outlives the circuit holding it: the part worth remembering is written to
/// the browser's local storage on every change and read back once the browser is reachable.
/// </summary>
/// <remarks>
/// Reading is deliberately not part of construction. The browser is reached over JavaScript
/// interop, which a circuit cannot use before its first render, so a caller starts with the
/// initial state and calls <see cref="InitializeAsync"/> once the component is interactive.
/// </remarks>
/// <typeparam name="TState">State the store hands out.</typeparam>
/// <typeparam name="TPersistedState">Projection of the state that is remembered.</typeparam>
/// <param name="localStorage">Browser storage the projection is kept in.</param>
/// <param name="logger">Log a storage failure is reported to.</param>
/// <param name="storageKey">Key the projection is stored under.</param>
/// <param name="initialState">State the store holds until the browser is read.</param>
public abstract class LocalStorageStoreBase<TState, TPersistedState>(
    ProtectedLocalStorage localStorage,
    IAppLogger logger,
    string storageKey,
    TState initialState)
    where TState : class
    where TPersistedState : class
{
    private bool isInitialized;

    /// <summary>
    /// Raised after the state has changed, on the thread of the caller that changed it.
    /// </summary>
    public event Action? Changed;

    /// <summary>
    /// Gets the state as it stands now.
    /// </summary>
    public TState Current { get; private set; } = initialState;

    /// <summary>
    /// Reads the remembered projection and folds it into the current state. Does nothing on any
    /// call after the first, so several components may ask without agreeing who asks first.
    /// </summary>
    /// <returns>A task that completes once the browser has answered.</returns>
    public async Task InitializeAsync()
    {
        if (this.isInitialized)
        {
            return;
        }

        this.isInitialized = true;

        TPersistedState? persistedState = await this.ReadAsync();

        if (persistedState is not null)
        {
            this.SetState(this.Restore(this.Current, persistedState));
        }
    }

    /// <summary>
    /// Replaces the state with the result of <paramref name="update"/> and remembers it.
    /// </summary>
    /// <param name="update">Produces the new state from the current one.</param>
    /// <returns>A task that completes once the browser has stored the projection.</returns>
    public async Task UpdateAsync(Func<TState, TState> update)
    {
        this.SetState(update(this.Current));

        await this.WriteAsync();
    }

    /// <summary>
    /// Takes the part of the state that is worth remembering.
    /// </summary>
    /// <param name="state">State to project.</param>
    /// <returns>The projection to store.</returns>
    protected abstract TPersistedState Persist(TState state);

    /// <summary>
    /// Folds a remembered projection back into the state.
    /// </summary>
    /// <param name="state">State to fold into.</param>
    /// <param name="persistedState">Projection read from storage.</param>
    /// <returns>The state carrying what was remembered.</returns>
    protected abstract TState Restore(TState state, TPersistedState persistedState);

    private void SetState(TState state)
    {
        this.Current = state;
        this.Changed?.Invoke();
    }

    private async Task<TPersistedState?> ReadAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<TPersistedState> result =
                await localStorage.GetAsync<TPersistedState>(storageKey);

            return result.Success ? result.Value : null;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // A store that cannot read the browser works without what was remembered rather than
            // break over it, but the user is told, because silently starting from a blank state
            // looks exactly like someone else having cleared the selection.
            logger.Warning(
                "The state remembered by the browser could not be read, starting from a blank one.",
                exception.ToString());

            return null;
        }
    }

    private async Task WriteAsync()
    {
        try
        {
            await localStorage.SetAsync(storageKey, this.Persist(this.Current));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.Warning(
                "The current state could not be remembered by the browser, so it is lost on reload.",
                exception.ToString());
        }
    }
}