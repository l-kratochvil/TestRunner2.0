namespace TestRunner.WebApp.Features.TestDiscovery.Components;

using System.Linq;
using TestRunner.Common.Model;

/// <summary>
/// The test tree as the explorer shows it: the discovered test suites turned into nodes, and the
/// selection made in them.
/// </summary>
/// <remarks>
/// Holds nothing but UI state and knows nothing about where the selection is kept, so the rules of
/// selecting can be exercised on their own.
/// </remarks>
public sealed class TestExplorerViewModel
{
    private TestExplorerViewModel(IReadOnlyList<TestNodeViewModel> roots)
    {
        this.Roots = roots;
    }

    /// <summary>Gets the nodes standing for the discovered test suites.</summary>
    public IReadOnlyList<TestNodeViewModel> Roots { get; }

    /// <summary>Gets a value indicating whether there is any test to show.</summary>
    public bool IsEmpty => this.Roots.Count == 0;

    /// <summary>
    /// Gets the execution paths of the selected test cases, which is what a test run is made of.
    /// </summary>
    public IReadOnlyList<string> SelectedTestCasesPaths
        => [.. this.SelectedTestCaseNodes().Select(node => node.ExecutionPath)];

    /// <summary>
    /// Builds the tree from the discovered test suites.
    /// </summary>
    /// <param name="testSuites">Test suites to show.</param>
    /// <returns>The tree.</returns>
    public static TestExplorerViewModel Create(IEnumerable<TestSuiteEntity> testSuites)
        => new([.. testSuites.Select(TestNodeViewModel.CreateFrom)]);

    /// <summary>
    /// Selects the test cases sitting at the given execution paths and clears every other one.
    /// Paths matching no test case are ignored, so a selection made before the test assemblies
    /// changed restores as much of itself as still exists.
    /// </summary>
    /// <param name="executionPaths">Execution paths of the test cases to select.</param>
    public void ApplySelection(IEnumerable<string> executionPaths)
    {
        var selectedPaths = executionPaths.ToHashSet(StringComparer.Ordinal);

        foreach (TestNodeViewModel node in this.AllNodes().Where(node => node.IsTestCase))
        {
            node.SetChecked(selectedPaths.Contains(node.ExecutionPath));
        }

        this.ExpandTowardsSelection();
    }

    private IEnumerable<TestNodeViewModel> AllNodes()
        => this.Roots.SelectMany(root => root.SelfAndDescendants());

    private IEnumerable<TestNodeViewModel> SelectedTestCaseNodes()
        => this.AllNodes().Where(
            node => node.IsTestCase && node.CheckState == TestNodeCheckState.Checked);

    private void ExpandTowardsSelection()
    {
        // A restored selection the user cannot see is indistinguishable from none, so every group
        // holding one is opened.
        foreach (TestNodeViewModel node in this.AllNodes())
        {
            if (node.HasChildren && node.CheckState != TestNodeCheckState.Unchecked)
            {
                node.IsExpanded = true;
            }
        }
    }
}