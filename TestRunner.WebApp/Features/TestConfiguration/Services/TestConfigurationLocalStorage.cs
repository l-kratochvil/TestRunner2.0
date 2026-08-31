namespace TestRunner.WebApp.Features.TestConfiguration.Services;

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TestRunner.WebApp.Shared.Logging;
using TestRunner.WebApp.Shared.Stores;

/// <summary>
/// The test run configuration as the browser remembers it between visits.
/// </summary>
/// <param name="localStorage">Browser storage the configuration is kept in.</param>
/// <param name="logger">Log a storage failure is reported to.</param>
public sealed class TestConfigurationLocalStorage(
    ProtectedLocalStorage localStorage,
    IAppLogger logger)
    : LocalStorageBase<TestConfigurationData>(localStorage, logger)
{
    /// <inheritdoc/>
    protected override string StorageName
        => "test-configuration";
}