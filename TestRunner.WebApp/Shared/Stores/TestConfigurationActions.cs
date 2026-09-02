namespace TestRunner.WebApp.Shared.Stores;

using TestRunner.WebApp.Shared.Domain;

public record ChangedAction(
    ValueChange<bool>? NewIsTestLinkEnabled,
    ValueChange<TestedHwAssemblyType?>? NewTestedHwAssembly,
    ValueChange<Version?>? NewIdeVersion,
    ValueChange<Version?>? NewRuntimeVersion);