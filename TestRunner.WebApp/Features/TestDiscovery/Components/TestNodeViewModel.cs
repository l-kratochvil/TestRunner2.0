namespace TestRunner.WebApp.Features.TestDiscovery.Components;

using System.Linq;
using TestRunner.Common.Model;

/// <summary>
/// One node of the test tree: what it is called, whether it is open, and how it is selected.
/// </summary>
/// <remarks>
/// Only test cases carry a selection of their own; a suite or a fixture reads its state off the
/// test cases beneath it. There is therefore nothing to keep in step — a parent cannot claim to be
/// selected while its children say otherwise, because it never holds an opinion of its own.
/// </remarks>
public sealed class TestNodeViewModel
{
    private readonly List<TestNodeViewModel> children = [];

    private bool isChecked;

    private TestNodeViewModel(TestEntity entity, bool isTestCase)
    {
        this.Name = entity.Name;
        this.ExecutionPath = entity.ExecutionPath;
        this.IsTestCase = isTestCase;
    }

    /// <summary>Gets the name shown to the user.</summary>
    public string Name { get; }

    /// <summary>Gets the NUnit path the node is identified and executed by.</summary>
    public string ExecutionPath { get; }

    /// <summary>Gets a value indicating whether the node is a test case rather than a group.</summary>
    public bool IsTestCase { get; }

    /// <summary>Gets the nodes nested under this one.</summary>
    public IReadOnlyList<TestNodeViewModel> Children => this.children;

    /// <summary>Gets or sets a value indicating whether the nested nodes are shown.</summary>
    public bool IsExpanded { get; set; }

    /// <summary>Gets a value indicating whether the node has anything nested under it.</summary>
    public bool HasChildren => this.children.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the node can be selected at all. An empty group has no test
    /// case to select and would be a control that does nothing.
    /// </summary>
    public bool CanCheck => this.IsTestCase || this.HasChildren;

    /// <summary>
    /// Gets how the node appears to be selected.
    /// </summary>
    public TestNodeCheckState CheckState
    {
        get
        {
            if (!this.HasChildren)
            {
                return this.isChecked ? TestNodeCheckState.Checked : TestNodeCheckState.Unchecked;
            }

            bool hasChecked = false;
            bool hasUnchecked = false;

            foreach (TestNodeViewModel child in this.children)
            {
                switch (child.CheckState)
                {
                    case TestNodeCheckState.Checked:
                        hasChecked = true;
                        break;

                    case TestNodeCheckState.Unchecked:
                        hasUnchecked = true;
                        break;

                    default:
                        return TestNodeCheckState.Mixed;
                }

                if (hasChecked && hasUnchecked)
                {
                    return TestNodeCheckState.Mixed;
                }
            }

            return hasChecked ? TestNodeCheckState.Checked : TestNodeCheckState.Unchecked;
        }
    }

    /// <summary>
    /// Builds the node and everything below it from a discovered test suite.
    /// </summary>
    /// <param name="testSuite">Test suite to build from.</param>
    /// <returns>The node standing for the test suite.</returns>
    public static TestNodeViewModel CreateFrom(TestSuiteEntity testSuite)
    {
        var node = new TestNodeViewModel(testSuite, isTestCase: false) { IsExpanded = true };

        node.children.AddRange(testSuite.TestFixtures.Select(CreateFrom));

        return node;
    }

    /// <summary>
    /// Selects or clears the node, and with it everything below it.
    /// </summary>
    /// <param name="isSelected">Whether the node should end up selected.</param>
    public void SetChecked(bool isSelected)
    {
        if (this.HasChildren)
        {
            foreach (TestNodeViewModel child in this.children)
            {
                child.SetChecked(isSelected);
            }

            return;
        }

        if (this.IsTestCase)
        {
            this.isChecked = isSelected;
        }
    }

    /// <summary>
    /// Flips the node: a partly selected group is completed rather than cleared, so the click that
    /// follows a partial selection adds to it instead of undoing work already done.
    /// </summary>
    public void Toggle()
        => this.SetChecked(this.CheckState != TestNodeCheckState.Checked);

    /// <summary>
    /// Walks the node and everything below it.
    /// </summary>
    /// <returns>The node followed by its descendants.</returns>
    public IEnumerable<TestNodeViewModel> SelfAndDescendants()
        => [this, .. this.children.SelectMany(child => child.SelfAndDescendants())];

    private static TestNodeViewModel CreateFrom(TestFixtureEntity testFixture)
    {
        var node = new TestNodeViewModel(testFixture, isTestCase: false);

        node.children.AddRange(testFixture.TestCases.Select(
            static testCase => new TestNodeViewModel(testCase, isTestCase: true)));

        return node;
    }
}