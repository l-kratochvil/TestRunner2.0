namespace TestRunner.WebApp.Features.TestDiscovery.Components;

/// <summary>
/// How a node of the test tree appears to be selected.
/// </summary>
public enum TestNodeCheckState
{
    /// <summary>Nothing beneath the node is selected.</summary>
    Unchecked,

    /// <summary>Some but not all of the test cases beneath the node are selected.</summary>
    Mixed,

    /// <summary>The node, or everything beneath it, is selected.</summary>
    Checked,
}