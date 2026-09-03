namespace TestRunner.WebApp.Shared.Stores;

using Fluxor;
using TestRunner.WebApp.Shared.Domain;

/// <summary>
/// The test run a tester is putting together, apart from the test selection.
/// </summary>
/// <remarks>
/// Belongs to one browser and is remembered there. Whether it can be run is checked through
/// <see cref="ITestConfigurationValidator"/>, because that also depends on the test selection.
/// <para>
/// The feature keeps the name of <see cref="TestConfigurationState"/> so Fluxor does not persist
/// the namespace into browser storage keys.
/// </para>
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
[FeatureState(Name = nameof(TestConfigurationState))]
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
}