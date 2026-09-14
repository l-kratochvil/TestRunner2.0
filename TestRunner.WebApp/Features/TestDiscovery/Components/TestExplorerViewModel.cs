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
/// <param name="testSuites">Test suites to show.</param>
public sealed class TestExplorerViewModel(IEnumerable<TestSuiteEntity> testSuites)
{
    /// <summary>Gets the nodes standing for the discovered test suites.</summary>
    public IReadOnlyList<TestTreeNodeData> Roots { get; } = [..testSuites.Select(TestTreeNodeData.Create)];

    /// <summary>Gets a value indicating whether there is any test to show.</summary>
    public bool IsEmpty
        => this.Roots.Count == 0;

    /// <summary>
    /// Gets the selected test cases, which is what a test run is made of.
    /// </summary>
    public IReadOnlyList<TestCaseEntity> SelectedTestCases
        => [..this.SelectedTestCaseNodes().Select(node => node.Entity).OfType<TestCaseEntity>()];

    /// <summary>
    /// Selects the test cases sitting at the given execution paths and clears every other one.
    /// Paths matching no test case are ignored, so a selection made before the test assemblies
    /// changed restores as much of itself as still exists.
    /// </summary>
    /// <param name="executionPaths">Execution paths of the test cases to select.</param>
    public void ApplySelection(IEnumerable<string> executionPaths)
    {
        var selectedPaths = executionPaths.ToHashSet(StringComparer.Ordinal);

        foreach (TestTreeNodeData node in this.AllNodes().Where(node => node.IsTestCase))
        {
            node.SetChecked(selectedPaths.Contains(node.ExecutionPath));
        }

        this.ExpandTowardsSelection();
    }

    private IEnumerable<TestTreeNodeData> AllNodes()
        => this.Roots.SelectMany(root => root.SelfAndDescendants());

    private IEnumerable<TestTreeNodeData> SelectedTestCaseNodes()
        => this.AllNodes().Where(
            node => node.IsTestCase && node.CheckState == TestTreeNodeData.State.Checked);

    private void ExpandTowardsSelection()
    {
        // A restored selection the user cannot see is indistinguishable from none, so every group
        // holding one is opened.
        foreach (TestTreeNodeData node in this.AllNodes())
        {
            if (node.HasChildren && node.CheckState != TestTreeNodeData.State.Unchecked)
            {
                node.IsExpanded = true;
            }
        }
    }
}