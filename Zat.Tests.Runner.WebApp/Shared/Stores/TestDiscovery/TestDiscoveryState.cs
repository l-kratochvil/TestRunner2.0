namespace Zat.Tests.Runner.WebApp.Shared.Stores.TestDiscovery;

using Zat.Tests.Runner.Common.Model;

/// <summary>
/// The current test selection.
/// </summary>
/// <param name="SelectedTestCases">
/// The selected test cases. Suites and fixtures are derived from these, so the test selection has
/// one source of truth.
/// </param>
public sealed record TestDiscoveryState(IReadOnlyList<TestCaseEntity> SelectedTestCases);