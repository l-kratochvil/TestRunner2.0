namespace TestRunner.WebApp.Shared.Stores;

/// <summary>
/// Checks whether a test configuration can be used for a test run.
/// </summary>
/// <remarks>
/// The TestConfiguration feature owns these rules so other features cannot define validity for the
/// same configuration differently.
/// </remarks>
public interface ITestConfigurationValidator
{
    /// <summary>
    /// Checks <paramref name="values"/>.
    /// </summary>
    /// <param name="values">Values to check.</param>
    /// <returns>The problems found in <paramref name="values"/>.</returns>
    TestConfigurationValidity Validate(TestConfigurationValues values);
}
