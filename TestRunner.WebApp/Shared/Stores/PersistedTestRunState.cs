namespace TestRunner.WebApp.Shared.Stores;

/// <summary>
/// The part of <see cref="TestRunState"/> the browser remembers between visits.
/// </summary>
/// <remarks>
/// The discovered test suites are deliberately absent: they are read from the test assemblies on
/// every start, so a copy kept next to them would be a second answer to the same question, and a
/// stale one as soon as a test is added.
/// </remarks>
/// <param name="SelectedTestCasesPaths">Execution paths of the selected test cases.</param>
/// <param name="IdeVersion">Version of the IDE the run is reported against.</param>
public sealed record PersistedTestRunState(
    string[] SelectedTestCasesPaths,
    string? IdeVersion);