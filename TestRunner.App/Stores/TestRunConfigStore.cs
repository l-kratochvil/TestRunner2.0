namespace TestRunner.App.Stores;

/// <summary>
/// This is the store for test run config that is used by the running test.
/// </summary>
/// <param name="appStateStore"></param>
internal class TestRunConfigStore(AppStateStore appStateStore)
{
    public string? RuntimeVersion
    {
        get => appStateStore.Current.RuntimeVersion;
        set => appStateStore.Update(current => current with { RuntimeVersion = value });
    }

    public string? TestStation
    {
        get => appStateStore.Current.TestStation;
        set => appStateStore.Update(current => current with { TestStation = value });
    }
}