namespace TestRunner.WebApp.Shared.Stores;

using TestRunner.Common.Model;

/// <summary>
/// What the user picked to run.
/// </summary>
/// <param name="SelectedTestCases">
/// The selected test cases. Only test cases are ever selected: what a suite or a fixture looks
/// like is derived from the test cases beneath it, so there is one description of what runs and it
/// cannot disagree with itself.
/// </param>
public sealed record TestDiscoveryState(IReadOnlyList<TestCaseEntity> SelectedTestCases);