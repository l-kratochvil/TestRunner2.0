namespace TestRunner.WebApp.Shared.Logging;

/// <summary>
/// Channels the application log is split into. Sources are plain strings so that entries coming
/// from outside the application (for example test runner output) can be routed without a mapping.
/// </summary>
public static class LogSources
{
    /// <summary>
    /// The application itself: lifecycle, navigation, settings.
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
    /// Gets all sources known up front, in the order they should be offered in the UI.
    /// </summary>
    public static IReadOnlyList<string> All { get; } = [App, TestRun, TestLink];
}