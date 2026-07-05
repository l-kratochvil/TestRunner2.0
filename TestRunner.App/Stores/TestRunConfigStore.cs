namespace TestRunner.App.Stores;

using System.Collections.Generic;
using TestRunner.Common.Model;

internal class TestRunConfigStore(AppStateStore appStateStore)
{
    public string? RuntimeVersion
    {
        get => appStateStore.Current.RuntimeVersion;
        set => appStateStore.Update(current => current with { RuntimeVersion = value });
    }

    public string? IdeVersion
    {
        get => appStateStore.Current.IdeVersion;
        set => appStateStore.Update(current => current with { IdeVersion = value });
    }

    public IEnumerable<TestEntity> TestEntities { get; set; } = [];

    public bool? IsRuntimeTest { get; set; } = false;
}