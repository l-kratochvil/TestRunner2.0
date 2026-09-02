namespace TestRunner.WebApp.Features.TestConfiguration;

using Fluxor;
using TestRunner.WebApp.Shared.Stores;

public static class Reducers
{
    [ReducerMethod]
    public static TestConfigurationState OnChanged(
        TestConfigurationState current, ChangedAction action)
    {
        var updated = new TestConfigurationState();

        if (action.NewRuntimeVersion is not null)
        {
            updated = current with { RuntimeVersion = action.NewRuntimeVersion };
        }

        if (action.NewIdeVersion is not null)
        {
            updated = current with { IdeVersion = action.NewIdeVersion };
        }

        return updated;
    }
}