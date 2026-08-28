namespace TestRunner.WebApp.Shared.Stores;

using System.Linq;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TestRunner.Common.Model;
using TestRunner.WebApp.Shared.Logging;

/// <summary>
/// The test run being put together: which tests were discovered, which of them the user selected,
/// and how the run is configured. Selecting happens in one browser tab and belongs to it, so the
/// store is scoped to a circuit; what the user selected last is remembered across reloads.
/// </summary>
/// <param name="localStorage">Browser storage the selection is remembered in.</param>
/// <param name="logger">Log a storage failure is reported to.</param>
/// <param name="loadedTestSuites">Test suites the user may choose from.</param>
public sealed class TestRunStore(
    ProtectedLocalStorage localStorage,
    IAppLogger logger,
    IEnumerable<TestSuiteEntity> loadedTestSuites)
    : LocalStorageStoreBase<TestRunState, PersistedTestRunState>(
        localStorage,
        logger,
        StorageKey,
        new TestRunState(
            LoadedTestSuites: [.. loadedTestSuites],
            SelectedTestCasesPaths: [],
            IsRuntimeTest: false,
            IdeVersion: null))
{
    /// <summary>
    /// Key the remembered part of the state is stored under.
    /// </summary>
    public const string StorageKey = "testrunner.test-run";

    /// <summary>
    /// Resolves the selection against the discovered test suites.
    /// </summary>
    /// <remarks>
    /// The store keeps execution paths rather than the test cases themselves, so nothing it holds
    /// can outlive the tree it came from: a path that no longer matches anything simply selects
    /// nothing, which is what a selection made before the test assemblies changed should do.
    /// </remarks>
    /// <returns>The selected test cases, as they exist in the discovered test suites.</returns>
    public IEnumerable<TestCaseEntity> GetSelectedTestCases()
    {
        var selectedPaths = this.Current.SelectedTestCasesPaths.ToHashSet(StringComparer.Ordinal);

        return this.Current.LoadedTestSuites
            .SelectMany(testSuite => testSuite.TestFixtures)
            .SelectMany(testFixture => testFixture.TestCases)
            .Where(testCase => selectedPaths.Contains(testCase.ExecutionPath));
    }

    /// <inheritdoc/>
    protected override PersistedTestRunState Persist(TestRunState state)
        => new(
            SelectedTestCasesPaths: [.. state.SelectedTestCasesPaths],
            IdeVersion: state.IdeVersion);

    /// <inheritdoc/>
    protected override TestRunState Restore(TestRunState state, PersistedTestRunState persistedState)
        => state with
        {
            SelectedTestCasesPaths = [.. persistedState.SelectedTestCasesPaths],
            IdeVersion = persistedState.IdeVersion,
        };
}