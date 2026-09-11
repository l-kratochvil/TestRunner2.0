namespace TestRunner.WebApp.Tests.Shared.TestConfiguration;

using System.Reflection;

using Fluxor;
using NUnit.Framework;
using TestRunner.WebApp.Shared.TestConfiguration;

[TestFixture]
public class TestConfigurationStateTests
{
    [Test]
    public void FeatureName__WhenFluxorNamesTheFeature__ThenShouldBePinnedRatherThanDerivedFromTheType()
    {
        // Given:
        // The persistence whitelist is matched against the feature name, so a name Fluxor derives
        // from the full type name would stop the configuration being remembered as soon as the
        // type moves, and would do so without a word.
        var result = typeof(TestConfigurationState).GetCustomAttribute<FeatureStateAttribute>();

        // Then:
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result?.Name, Is.EqualTo(TestConfigurationState.FeatureName));
            Assert.That(
                TestConfigurationState.FeatureName,
                Is.Not.EqualTo(typeof(TestConfigurationState).FullName));
        }
    }
}
