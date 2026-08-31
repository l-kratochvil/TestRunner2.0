namespace TestRunner.WebApp.Features.TestConfiguration.Services;

using TestRunner.WebApp.Shared.Stores;

/// <summary>
/// How the test run being put together is configured. Configuring happens in one browser tab and
/// belongs to it, so the store is scoped to a circuit; what was configured last is remembered
/// across reloads.
/// </summary>
/// <param name="localStorage">Browser storage the configuration is remembered in.</param>
public sealed class TestConfigurationStore(TestConfigurationLocalStorage localStorage)
    : StoreBase<TestConfigurationState>, ITestConfigurationStore
{
    /// <inheritdoc/>
    protected override TestConfigurationState DefaultState
        => new(IdeVersion: null);

    /// <summary>
    /// Reads what the browser remembers about the configuration and puts it back, without
    /// remembering it again.
    /// </summary>
    /// <returns>A task that completes once the browser has answered.</returns>
    public async Task RestoreAsync()
    {
        TestConfigurationData? data = await localStorage.ReadAsync();

        if (data is not null)
        {
            this.SetState(new TestConfigurationState(data.IdeVersion));
        }
    }

    /// <summary>
    /// Changes the configuration and has the browser remember it.
    /// </summary>
    /// <param name="update">Produces the new configuration from the current one.</param>
    /// <returns>A task that completes once the browser has stored the configuration.</returns>
    public override async Task UpdateAsync(
        Func<TestConfigurationState, TestConfigurationState> update)
    {
        await base.UpdateAsync(update);

        await localStorage.WriteAsync(new TestConfigurationData(this.Current.IdeVersion));
    }
}