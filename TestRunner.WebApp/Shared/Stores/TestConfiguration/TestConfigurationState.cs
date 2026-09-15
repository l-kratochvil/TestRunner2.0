namespace TestRunner.WebApp.Shared.Stores.TestConfiguration;

using System.Text.Json.Serialization;

using Fluxor;
using TestRunner.WebApp.Shared.Domain;

/// <summary>
/// The test run a tester is putting together, apart from the test selection.
/// </summary>
/// <remarks>
/// Belongs to one browser and is remembered there.
/// </remarks>
/// <param name="IsTestLinkEnabled">Whether the test result is written to TestLink.</param>
/// <param name="IdeVersion">
/// IDE version the test result is filed under in TestLink. Only validated versions are kept here.
/// </param>
/// <param name="RuntimeVersion">
/// Runtime version the test run uses. This is the installation folder name, because that is the
/// runtime version identifier elsewhere in the application.
/// </param>
/// <param name="TestedHwAssembly">Test station the test run uses.</param>
[FeatureState]
public record TestConfigurationState(
    bool IsTestLinkEnabled,
    Version? IdeVersion,
    string? RuntimeVersion,
    TestedHwAssemblyType? TestedHwAssembly)
{
    public TestConfigurationState()
        : this(false, null, null, null)
    {
    }

    /// <summary>
    /// Gets a value indicating whether a test run can be started with this configuration.
    /// </summary>
    /// <remarks>
    /// Filled in by the configurator, which is the only place the rules are run: they also take the
    /// test selection into account, which the configuration itself knows nothing about. Nothing is
    /// runnable until it says otherwise, which is also what a configuration restored from the
    /// browser comes back as, because the answer is not remembered with it.
    /// </remarks>
    [JsonIgnore]
    public bool IsValid { get; init; }
}