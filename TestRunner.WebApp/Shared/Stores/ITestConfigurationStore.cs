namespace TestRunner.WebApp.Shared.Stores;

/// <summary>
/// How the test run is configured, as the features that do not configure it read it.
/// </summary>
public interface ITestConfigurationStore
{
    /// <summary>
    /// Raised after the configuration has changed.
    /// </summary>
    event Action? Changed;

    /// <summary>
    /// Gets the configuration as it stands now.
    /// </summary>
    TestConfigurationState Current { get; }
}