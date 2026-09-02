namespace TestRunner.WebApp.Shared.Stores;

using TestRunner.WebApp.Shared.Domain;

/// <summary>
/// Changes part of the test configuration. What is not given is left as it stands, so that
/// changing one field does not have to restate the rest of the configuration.
/// </summary>
/// <param name="NewIsTestLinkEnabled">Whether the result is written to TestLink.</param>
/// <param name="NewTestedHwAssembly">Test station the tests run on.</param>
/// <param name="NewIdeVersion">Version of the IDE the result is filed under.</param>
/// <param name="NewRuntimeVersion">Runtime version the tests run against.</param>
public record ChangedAction(
    ValueChange<bool>? NewIsTestLinkEnabled = null,
    ValueChange<TestedHwAssemblyType?>? NewTestedHwAssembly = null,
    ValueChange<Version?>? NewIdeVersion = null,
    ValueChange<string?>? NewRuntimeVersion = null);

/// <summary>
/// Says that the configuration remembered by the browser has been put back and looked over.
/// </summary>
/// <remarks>
/// Raised after whatever no longer holds has been dropped from the restored configuration, so that
/// the configurator can show it without having to know whether the checking has happened yet.
/// </remarks>
public record RestoredAction;