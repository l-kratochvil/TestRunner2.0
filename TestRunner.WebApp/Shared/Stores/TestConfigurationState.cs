namespace TestRunner.WebApp.Shared.Stores;

using Fluxor;
using TestRunner.WebApp.Shared.Domain;

/// <summary>
/// The test run the tester is putting together: everything that is not the tests themselves.
/// </summary>
/// <remarks>
/// Belongs to one browser and is remembered in it, so a tester finds their last run set up as they
/// left it. Whether it may be run is not kept here — see <see cref="ITestConfigurationValidator"/>,
/// because that also depends on which tests were selected.
/// <para>
/// The feature is named after the type on purpose: left alone, Fluxor names it after the full name
/// of the state, which the list of what is worth remembering would then have to spell out, spelling
/// the namespace into a place that has no business knowing it. See <c>InitServicesExtension</c>.
/// </para>
/// </remarks>
/// <param name="IsTestLinkEnabled">Whether the result is written to TestLink.</param>
/// <param name="IdeVersion">
/// Version of the IDE the result is filed under in TestLink. Only ever set to a version that was
/// found to be well formed; what the tester is still typing lives in the configurator.
/// </param>
/// <param name="RuntimeVersion">
/// Runtime version the tests run against. The name of the folder it is installed in rather than a
/// number, because that name is what identifies the installation everywhere else.
/// </param>
/// <param name="TestedHwAssembly">Test station the tests run on.</param>
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