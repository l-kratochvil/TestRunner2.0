namespace TestRunner.WebApp.Shared.Stores;

/// <summary>
/// The part of <see cref="TestConfigurationState"/> the browser remembers between visits.
/// </summary>
/// <param name="IdeVersion">Version of the IDE the run is reported against.</param>
public sealed record TestConfigurationData(string? IdeVersion);