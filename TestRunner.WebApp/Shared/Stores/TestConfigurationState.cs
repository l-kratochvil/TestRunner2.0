namespace TestRunner.WebApp.Shared.Stores;

using Fluxor;
using TestRunner.WebApp.Shared.Domain;

[FeatureState]
public record TestConfigurationState(
    bool IsTestLinkEnabled,
    Version? IdeVersion,
    Version? RuntimeVersion,
    TestedHwAssemblyType? TestedHwAssembly)
{
    public TestConfigurationState()
        : this(false, null, null, null)
    {
    }
}