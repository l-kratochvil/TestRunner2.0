namespace TestRunner.WebApp.Tests.Components.Primitives;

using NUnit.Framework;

using TestRunner.WebApp.Components.Primitives;

/// <summary>
/// The one place a binding's event can be misspelled.
/// </summary>
[TestFixture]
public class BindingEventExtensionsTests
{
    [TestCase(BindingEvent.OnChange, "onchange")]
    [TestCase(BindingEvent.OnInput, "oninput")]
    public void ToEventName__WhenAnOfferedEventIsNamed__ThenShouldSpellItAsTheBrowserDoes(
        BindingEvent givenEvent, string expectedName)
    {
        // When:
        var name = givenEvent.ToEventName();

        // Then:
        Assert.That(name, Is.EqualTo(expectedName));
    }

    [Test]
    public void ToEventName__WhenTheEventIsNoneOfTheOffered__ThenShouldThrow()

        // Nothing checks the name a binding is given, so a value from outside the offer would
        // otherwise show up as a control the tester's edits never reach.
        => Assert.That(
            () => ((BindingEvent)(-1)).ToEventName(),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
}
