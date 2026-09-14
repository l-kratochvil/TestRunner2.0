namespace TestRunner.WebApp.Features.TestConfiguration;

using Fluxor;
using TestRunner.WebApp.Shared.Stores.TestConfiguration;

public static class Reducers
{
    [ReducerMethod]
    public static TestConfigurationState OnChanged(
        TestConfigurationState current, ChangedAction action)
    {
        var updated = current;

        updated = UpdateIfChanged(
            updated,
            action.NewRuntimeVersion,
            (state, value) => state with { RuntimeVersion = value });
        updated = UpdateIfChanged(
            updated,
            action.NewIdeVersion,
            (state, value) => state with { IdeVersion = value });
        updated = UpdateIfChanged(
            updated,
            action.NewIsTestLinkEnabled,
            (state, value) => state with { IsTestLinkEnabled = value });
        updated = UpdateIfChanged(
            updated,
            action.NewTestedHwAssembly,
            (state, value) => state with { TestedHwAssembly = value });

        return updated;
    }

    private static TestConfigurationState UpdateIfChanged<TValue>(
        TestConfigurationState current,
        ValueChange<TValue>? change,
        Func<TestConfigurationState, TValue, TestConfigurationState> update)
    {
        if (change is not null)
        {
            return update(current, change.Value);
        }

        return current;
    }
}