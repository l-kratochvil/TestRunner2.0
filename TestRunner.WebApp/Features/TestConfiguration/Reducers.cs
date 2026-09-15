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

    /// <summary>
    /// Takes over the values the change carries and leaves the rest as it stands.
    /// </summary>
    /// <param name="current">Configuration as it stands.</param>
    /// <param name="action">The change to take over.</param>
    /// <returns>Configuration with the changed values taken over.</returns>
    [ReducerMethod]
    public static TestConfigurationState OnChanged(
        TestConfigurationState current, ChangedAction action)
        => Updater
            .UpdateIfChanged(
                current with { IsValid = action.IsValid },
                action.NewRuntimeVersion,
                (state, value) => state with { RuntimeVersion = value })
            .UpdateIfChanged(
                action.NewIdeVersion,
                (state, value) => state with { IdeVersion = value })
            .UpdateIfChanged(
                action.NewIsTestLinkEnabled,
                (state, value) => state with { IsTestLinkEnabled = value })
            .UpdateIfChanged(
                action.NewTestedHwAssembly,
                (state, value) => state with { TestedHwAssembly = value })
            .Complete();
}