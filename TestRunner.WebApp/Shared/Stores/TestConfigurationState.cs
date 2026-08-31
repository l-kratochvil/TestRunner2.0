namespace TestRunner.WebApp.Shared.Stores;

/// <summary>
/// How the test run is configured.
/// </summary>
/// <param name="IdeVersion">Version of the IDE the run is reported against.</param>
public sealed record TestConfigurationState(string? IdeVersion);