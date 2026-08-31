namespace TestRunner.WebApp.Features.TestDiscovery.Services;

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TestRunner.WebApp.Shared.Logging;
using TestRunner.WebApp.Shared.Stores;

/// <summary>
/// The test selection as the browser remembers it between visits.
/// </summary>
/// <param name="localStorage">Browser storage the selection is kept in.</param>
/// <param name="logger">Log a storage failure is reported to.</param>
public sealed class TestDiscoveryLocalStorage(ProtectedLocalStorage localStorage, IAppLogger logger)
    : LocalStorageBase<TestDiscoveryData>(localStorage, logger)
{
    /// <inheritdoc/>
    protected override string StorageName
        => "test-discovery";
}