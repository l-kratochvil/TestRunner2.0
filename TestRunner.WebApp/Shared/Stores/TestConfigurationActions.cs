namespace TestRunner.WebApp.Shared.Stores;

using TestRunner.WebApp.Shared.Domain;

/// <summary>
/// Changes part of the test configuration without restating the rest.
/// </summary>
/// <param name="NewIsTestLinkEnabled">Whether the test result is written to TestLink.</param>
/// <param name="NewTestedHwAssembly">Test station the test run uses.</param>
/// <param name="NewIdeVersion">IDE version the test result is filed under.</param>
/// <param name="NewRuntimeVersion">Runtime version the test run uses.</param>
public record ChangedAction(
    ValueChange<bool>? NewIsTestLinkEnabled = null,
    ValueChange<TestedHwAssemblyType?>? NewTestedHwAssembly = null,
    ValueChange<Version?>? NewIdeVersion = null,
    ValueChange<string?>? NewRuntimeVersion = null);

/// <summary>
/// Says the remembered test configuration has been restored and revalidated.
/// </summary>
/// <remarks>
/// Raised after invalid remembered values have been dropped, so the configurator never sees stale
/// state.
/// </remarks>
public record RestoredAction;