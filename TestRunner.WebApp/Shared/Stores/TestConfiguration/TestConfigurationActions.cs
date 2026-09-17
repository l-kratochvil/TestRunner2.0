namespace TestRunner.WebApp.Shared.Stores.TestConfiguration;

using Zat.Z2xxTests.Common;

public record StatusChangedAction(
    ValueChange<bool>? NewHasErrors);

public record DataChangedAction(
    ValueChange<bool>? NewIsTestLinkReportEnabled = null,
    ValueChange<TestedHwAssemblyType?>? NewTestedHwAssembly = null,
    ValueChange<Version?>? NewIdeVersion = null,
    ValueChange<string?>? NewRuntimeVersion = null);