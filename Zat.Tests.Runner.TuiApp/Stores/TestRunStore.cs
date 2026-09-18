namespace Zat.Tests.Runner.TuiApp.Stores;

using System.Collections.Generic;

using Zat.Tests.Runner.Common.Model;

// TODO: Improve the name
/// <summary>
/// The store the test run used by application.
/// </summary>
internal class TestRunStore(AppStateStore appStateStore)
{
    public IEnumerable<TestSuiteEntity> LoadedTestSuites { get; set; } = [];

    public IEnumerable<TestEntity> SelectedTestEntities { get; set; } = [];

    public bool? IsRuntimeTest { get; set; } = false;

    public string? IdeVersion
    {
        get => appStateStore.Current.IdeVersion;
        set => appStateStore.Update(current => current with { IdeVersion = value });
    }
}