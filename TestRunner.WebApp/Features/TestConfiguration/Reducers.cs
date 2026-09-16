namespace TestRunner.WebApp.Features.TestConfiguration;

using Fluxor;

using TestRunner.WebApp.Shared.Stores;
using TestRunner.WebApp.Shared.Stores.TestConfiguration;

/// <summary>
/// How the test configuration answers what has been changed about it.
/// </summary>
public static class Reducers
{
    private static readonly StateUpdater<TestConfigurationState> Updater = new();

    [ReducerMethod]
    public static TestConfigurationState OnDataChanged(
        TestConfigurationState current, DataChangedAction action)
        => Updater
            .UpdateIfChanged(
                current,
                action.NewRuntimeVersion,
                (state, value) => state with { RuntimeVersion = value })
            .UpdateIfChanged(
                action.NewIdeVersion,
                (state, value) => state with { IdeVersion = value })
            .UpdateIfChanged(
                action.NewIsWriteToTestLinkEnabled,
                (state, value) => state with { IsWriteToTestLinkEnabled = value })
            .UpdateIfChanged(
                action.NewTestedHwAssembly,
                (state, value) => state with { TestedHwAssembly = value })
            .Complete();

    [ReducerMethod]
    public static TestConfigurationState OnStatusChanged(
        TestConfigurationState current, StatusChangedAction action)
        => Updater
            .UpdateIfChanged(
                current,
                action.NewHasErrors,
                (state, value) => state with { HasErrors = value })
            .Complete();
}