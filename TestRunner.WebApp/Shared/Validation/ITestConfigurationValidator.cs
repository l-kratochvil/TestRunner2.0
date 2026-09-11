namespace TestRunner.WebApp.Shared.Validation;

/// <summary>
/// Checks whether a test configuration can be used for a test run.
/// </summary>
/// <remarks>
/// The TestConfiguration feature owns these rules so other features cannot define validity for the
/// same configuration differently. It answers in <see cref="Validity"/> rather than in the result of
/// whatever rules library writes the rules: the library stays an implementation detail of the
/// validator, and the components need a lookup by field and a one-line summary, which the library's
/// result does not offer.
/// </remarks>
public interface ITestConfigurationValidator
{
    /// <summary>
    /// Checks <paramref name="values"/>.
    /// </summary>
    /// <param name="values">Values to check.</param>
    /// <returns>The problems found in <paramref name="values"/>.</returns>
    Validity Validate(TestConfigurationValues values);
}
