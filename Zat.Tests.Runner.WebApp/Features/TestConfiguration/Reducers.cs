namespace Zat.Tests.Runner.WebApp.Features.TestConfiguration;

using Fluxor;

using Zat.Tests.Runner.WebApp.Shared.Stores;
using Zat.Tests.Runner.WebApp.Shared.Stores.TestConfiguration;

/// <summary>
/// How the test configuration answers what has been changed about it.
/// </summary>
// ReSharper disable once UnusedMember.Global
public static class Reducers
{
    private static readonly StateUpdater<TestConfigurationState> Updater = new();

    [ReducerMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1600:Elements should be documented",
        Justification = "Fluxor reducer")]
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
                action.NewIsTestLinkReportEnabled,
                (state, value) => state with { IsTestLinkReportEnabled = value })
            .UpdateIfChanged(
                action.NewTestedHwAssembly,
                (state, value) => state with { TestedHwAssembly = value })
            .Complete();

    [ReducerMethod]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1600:Elements should be documented",
        Justification = "Fluxor reducer")]
    public static TestConfigurationState OnStatusChanged(
        TestConfigurationState current, StatusChangedAction action)
        => Updater
            .UpdateIfChanged(
                current,
                action.NewHasErrors,
                (state, value) => state with { HasErrors = value })
            .Complete();
}