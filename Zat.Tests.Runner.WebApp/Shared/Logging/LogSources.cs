namespace Zat.Tests.Runner.WebApp.Shared.Logging;

/// <summary>
/// Known log sources, kept as plain strings so outside entries can be routed without a mapping.
/// </summary>
public static class LogSources
{
    /// <summary>
    /// The application itself: lifecycle, navigation, and settings.
    /// </summary>
    public const string App = "App";

    /// <summary>
    /// A test run and the output of the test runner.
    /// </summary>
    public const string TestRun = "TestRun";

    /// <summary>
    /// Communication with TestLink.
    /// </summary>
    public const string TestLink = "TestLink";

    /// <summary>
    /// Gets the known sources in UI order.
    /// </summary>
    public static IReadOnlyList<string> All { get; } = [App, TestRun, TestLink];
}