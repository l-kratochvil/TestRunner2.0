namespace TestRunner.WebApp.Features.TestConfiguration.Services;

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TestRunner.WebApp.Shared.Logging;
using TestRunner.WebApp.Shared.Stores;

/// <summary>
/// How the test run being put together is configured. Configuring happens in one browser tab and
/// belongs to it, so the store is scoped to a circuit; what was configured last is remembered
/// across reloads.
/// </summary>
/// <param name="localStorage">Browser storage the configuration is remembered in.</param>
public sealed class TestConfigurationStore(
    IAppLogger logger,
    ProtectedLocalStorage protectedLocalStorage)
    : StoreBase<TestConfigurationStoreState>, ITestConfigurationStore
{
    private readonly LocalStorage<LocalStorageData> localStorage =
        new("test-configuration", protectedLocalStorage, logger);

    /// <inheritdoc/>
    protected override TestConfigurationStoreState DefaultState
        => new(IdeVersion: null);

    /// <summary>
    /// Reads what the browser remembers about the configuration and puts it back, without
    /// remembering it again.
    /// </summary>
    /// <returns>A task that completes once the browser has answered.</returns>
    public async Task RestoreAsync()
    {
        LocalStorageData? data = await this.localStorage.ReadAsync();

        if (data is not null)
        {
            this.SetState(new TestConfigurationStoreState(data.IdeVersion));
        }
    }

    /// <summary>
    /// Changes the configuration and has the browser remember it.
    /// </summary>
    /// <param name="update">Produces the new configuration from the current one.</param>
    /// <returns>A task that completes once the browser has stored the configuration.</returns>
    public override async Task UpdateAsync(
        Func<TestConfigurationStoreState, TestConfigurationStoreState> update)
    {
        await base.UpdateAsync(update);

        await this.localStorage.WriteAsync(new LocalStorageData(this.Current.IdeVersion));
    }

    private sealed record LocalStorageData(string? IdeVersion);
}