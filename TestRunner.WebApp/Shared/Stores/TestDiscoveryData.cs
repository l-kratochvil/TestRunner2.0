namespace TestRunner.WebApp.Shared.Stores;

/// <summary>
/// The part of <see cref="TestDiscoveryState"/> the browser remembers between visits.
/// </summary>
/// <remarks>
/// The selection is remembered as execution paths rather than as the test cases themselves, so
/// that nothing remembered can outlive the test tree it came from: a path that no longer matches
/// anything simply selects nothing, which is what a selection made before the test assemblies
/// changed should do.
/// </remarks>
/// <param name="SelectedTestCasesPaths">Execution paths of the selected test cases.</param>
public sealed record TestDiscoveryData(string[] SelectedTestCasesPaths);