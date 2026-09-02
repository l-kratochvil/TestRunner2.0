namespace TestRunner.WebApp.Shared.Stores;

using Fluxor;

[FeatureState]
public record TestConfigurationState(
    Version IdeVersion,
    Version RuntimeVersion)
{
    public TestConfigurationState()
        : this(new Version(), new Version())
    {
    }
}