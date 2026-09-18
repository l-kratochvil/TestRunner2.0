namespace Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;

/// <summary>
/// Store of the test selection for features that only read it.
/// </summary>
/// <remarks>
/// The TestDiscovery feature owns changing the test selection and exposes only this read-only view
/// to other features.
/// </remarks>
public interface ITestDiscoveryStore
{
    /// <summary>
    /// Raised after the test selection changes.
    /// </summary>
    event Action<TestDiscoveryState>? Changed;

    /// <summary>
    /// Gets the current test selection.
    /// </summary>
    TestDiscoveryState Current { get; }
}