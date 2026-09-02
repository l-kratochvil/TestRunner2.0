namespace TestRunner.WebApp.Shared.Storage;

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TestRunner.WebApp.Shared.Logging;

/// <typeparam name="TData">Shape of the remembered data.</typeparam>
/// <param name="protectedLocalStorage">Browser storage the data is kept in.</param>
/// <param name="logger">Log a storage failure is reported to.</param>
public class NamedLocalStorage<TData>(
    string storageName,
    ProtectedLocalStorage protectedLocalStorage,
    IAppLogger logger,
    Func<TData> fallbackFactory)
    where TData : class
{
    private readonly LocalStorage<TData> localStorage = new(
        protectedLocalStorage, logger, fallbackFactory);

    /// <summary>
    /// Reads what the browser remembers.
    /// </summary>
    /// <returns>The remembered data.</returns>
    public async Task<TData> ReadAsync()
        => await this.localStorage.GetItemAsync(storageName);

    /// <summary>
    /// Hands the data to the browser to remember.
    /// </summary>
    /// <param name="data">Data to remember.</param>
    /// <returns>A task that completes once the browser has stored the data.</returns>
    public async Task WriteAsync(TData data)
        => await this.localStorage.SetItemAsync(storageName, data);
}