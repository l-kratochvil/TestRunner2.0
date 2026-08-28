namespace TestRunner.WebApp.Shared.Stores;

using TestRunner.Common.Model;

/// <summary>
/// What the application knows about the test run being put together: the tests it may choose
/// from, the ones chosen, and how the run is configured.
/// </summary>
/// <param name="LoadedTestSuites">Test suites discovered from the test assemblies.</param>
/// <param name="SelectedTestCasesPaths">
/// Execution paths of the selected test cases. Only test cases are selected: what a suite or a
/// fixture looks like is derived from the test cases beneath it, so there is one description of
/// what runs and it cannot disagree with itself.
/// </param>
/// <param name="IsRuntimeTest">Whether the run targets the runtime rather than an application.</param>
/// <param name="IdeVersion">Version of the IDE the run is reported against.</param>
public sealed record TestRunState(
    IReadOnlyList<TestSuiteEntity> LoadedTestSuites,
    IReadOnlyList<string> SelectedTestCasesPaths,
    bool? IsRuntimeTest,
    string? IdeVersion);