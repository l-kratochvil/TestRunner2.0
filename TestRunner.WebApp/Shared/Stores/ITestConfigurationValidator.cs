namespace TestRunner.WebApp.Shared.Stores;

/// <summary>
/// Says whether a test configuration is one that tests can be run with.
/// </summary>
/// <remarks>
/// The one description of what a valid configuration is, so that the configurator and whoever
/// starts a test run cannot disagree about it. Implemented by the TestConfiguration feature, which
/// owns the rules; everyone else is handed this.
/// </remarks>
public interface ITestConfigurationValidator
{
    /// <summary>
    /// Checks a configuration.
    /// </summary>
    /// <param name="values">Values to check, whether typed or already put together.</param>
    /// <returns>What was found wrong, if anything.</returns>
    TestConfigurationValidity Validate(TestConfigurationValues values);
}
