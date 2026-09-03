namespace TestRunner.WebApp.Shared.Stores;

using TestRunner.WebApp.Shared.Domain;

/// <summary>
/// The values a test configuration is validated from.
/// </summary>
/// <remarks>
/// Held apart from <see cref="TestConfigurationState"/> because the IDE version is raw text here
/// until validation accepts it, and whether a test station is required comes from the current test
/// selection rather than duplicated state.
/// </remarks>
/// <param name="RuntimeVersion">Runtime version the test run uses.</param>
/// <param name="TestedHwAssembly">Test station the test run uses.</param>
/// <param name="IsTestLinkEnabled">Whether the test result is written to TestLink.</param>
/// <param name="IdeVersionText">IDE version text, empty while nothing has been typed.</param>
/// <param name="IsRuntimeTestSelected">Whether the test selection contains a runtime test case.</param>
public sealed record TestConfigurationValues(
    string? RuntimeVersion,
    TestedHwAssemblyType? TestedHwAssembly,
    bool IsTestLinkEnabled,
    string? IdeVersionText,
    bool IsRuntimeTestSelected)
{
    /// <summary>
    /// Reads validation values from <paramref name="state"/>.
    /// </summary>
    /// <param name="state">Configuration to read.</param>
    /// <param name="isRuntimeTestSelected">Whether the test selection contains a runtime test case.</param>
    /// <returns>The validation values for <paramref name="state"/>.</returns>
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
