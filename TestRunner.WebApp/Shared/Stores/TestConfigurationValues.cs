namespace TestRunner.WebApp.Shared.Stores;

using TestRunner.WebApp.Shared.Domain;

/// <summary>
/// The values a test configuration is made of, as they are checked.
/// </summary>
/// <remarks>
/// Held apart from <see cref="TestConfigurationState"/> for two reasons. The IDE version is a
/// string here, because what the tester is typing is only a version once it has been found to be
/// one, and checking has to be able to say that it is not yet. And whether a test station is needed
/// follows from the tests that were selected, which the configuration itself cannot see: the caller
/// knows the selection and says so, rather than the configuration keeping a copy that can go stale.
/// </remarks>
/// <param name="RuntimeVersion">Runtime version the tests are to run against.</param>
/// <param name="TestedHwAssembly">Test station the tests are to run on.</param>
/// <param name="IsTestLinkEnabled">Whether the result is to be written to TestLink.</param>
/// <param name="IdeVersionText">IDE version as text, empty while nothing has been typed.</param>
/// <param name="IsRuntimeTestSelected">Whether any selected test case is a runtime test.</param>
public sealed record TestConfigurationValues(
    string? RuntimeVersion,
    TestedHwAssemblyType? TestedHwAssembly,
    bool IsTestLinkEnabled,
    string? IdeVersionText,
    bool IsRuntimeTestSelected)
{
    /// <summary>
    /// Reads the values out of a configuration that was already put together.
    /// </summary>
    /// <param name="state">Configuration to read.</param>
    /// <param name="isRuntimeTestSelected">Whether any selected test case is a runtime test.</param>
    /// <returns>The values, ready to be checked.</returns>
    public static TestConfigurationValues From(
        TestConfigurationState state, bool isRuntimeTestSelected)
    {
        ArgumentNullException.ThrowIfNull(state);

        return new TestConfigurationValues(
            state.RuntimeVersion,
            state.TestedHwAssembly,
            state.IsTestLinkEnabled,
            state.IdeVersion?.ToString(),
            isRuntimeTestSelected);
    }
}
