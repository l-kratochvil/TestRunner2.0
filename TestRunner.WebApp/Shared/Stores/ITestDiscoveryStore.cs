namespace TestRunner.WebApp.Shared.Stores;

/// <summary>
/// The test selection, as the features that did not make it read it.
/// </summary>
/// <remarks>
/// Reading only. Selecting tests is what the TestDiscovery feature is for, so it owns the store
/// that changes the selection and nobody else is handed a way to.
/// </remarks>
public interface ITestDiscoveryStore
{
    /// <summary>
    /// Raised after the selection has changed.
    /// </summary>
    event Action? Changed;

    /// <summary>
    /// Gets the selection as it stands now.
    /// </summary>
    TestDiscoveryState Current { get; }
}