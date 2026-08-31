namespace TestRunner.WebApp.Shared.Stores;

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TestRunner.WebApp.Shared.Logging;

/// <summary>
/// A piece of data the browser remembers between visits, on behalf of a store.
/// </summary>
/// <remarks>
/// Reading is deliberately not part of construction. The browser is reached over JavaScript
/// interop, which a circuit cannot use before its first render, so a caller reads once the
/// component is interactive.
/// </remarks>
/// <typeparam name="TData">Shape of the remembered data.</typeparam>
/// <param name="localStorage">Browser storage the data is kept in.</param>
/// <param name="logger">Log a storage failure is reported to.</param>
public abstract class LocalStorageBase<TData>(ProtectedLocalStorage localStorage, IAppLogger logger)
    where TData : class
{
    /// <summary>
    /// Start of every key this application writes, so that its data is recognisable among whatever
    /// else shares the browser's storage for this origin.
    /// </summary>
    private const string KeyPrefix = "testrunner";

    /// <summary>
    /// Gets the name telling this data apart from the rest of the application's, which is all a
    /// derived class has to decide: the key itself is built here, so it cannot be spelled two ways.
    /// </summary>
    protected abstract string StorageName { get; }

    private string StorageKey
        => $"{KeyPrefix}.{this.StorageName}";

    /// <summary>
    /// Reads what the browser remembers.
    /// </summary>
    /// <returns>The remembered data, or <see langword="null"/> when there is none to be had.</returns>
    public async Task<TData?> ReadAsync()
    {
        try
        {
            ProtectedBrowserStorageResult<TData> result =
                await localStorage.GetAsync<TData>(this.StorageKey);

            return result.Success ? result.Value : null;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Working without what was remembered beats breaking over it, but the user is told,
            // because silently starting from a blank state looks exactly like someone else having
            // cleared it.
            logger.Warning(
                "The state remembered by the browser could not be read, starting from a blank one.",
                exception.ToString());

            return null;
        }
    }

    /// <summary>
    /// Hands the data to the browser to remember.
    /// </summary>
    /// <param name="data">Data to remember.</param>
    /// <returns>A task that completes once the browser has stored the data.</returns>
    public async Task WriteAsync(TData data)
    {
        try
        {
            await localStorage.SetAsync(this.StorageKey, data);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.Warning(
                "The current state could not be remembered by the browser, so it is lost on reload.",
                exception.ToString());
        }
    }
}