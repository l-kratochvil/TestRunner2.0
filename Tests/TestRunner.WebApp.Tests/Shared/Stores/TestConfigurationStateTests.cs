namespace TestRunner.WebApp.Tests.Shared.Stores;

using System.Reflection;

using Fluxor;

using NUnit.Framework;

using TestRunner.WebApp.Shared.Stores;

[TestFixture]
public class TestConfigurationStateTests
{
    [Test]
    public void FeatureState__WhenTheFeatureIsNamed__ThenShouldBeNamedAfterTheStateItself()
    {
        // Given:
        // What the browser remembers is picked by feature name (see InitServicesExtension), and
        // Fluxor names a feature after the *full* name of its state unless the state says
        // otherwise. Nothing complains when the two disagree: the configuration is simply never
        // remembered, which is only noticed by a tester wondering where their last run went.
        var attribute = typeof(TestConfigurationState).GetCustomAttribute<FeatureStateAttribute>();

        // Then:
        Assert.That(attribute?.Name, Is.EqualTo(nameof(TestConfigurationState)));
    }
}
