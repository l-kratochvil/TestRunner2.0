namespace TestRunner.WebApp.Tests.Components.Primitives;

using Bunit;

using Fluxor;

using Microsoft.Extensions.DependencyInjection;

using Moq;
using NUnit.Framework;

using TestRunner.WebApp.Components.Primitives;

/// <summary>
/// What the tick box shows and what a tick writes back.
/// </summary>
[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class BindingCheckboxTests : Bunit.TestContext
{
    private const string CheckboxSelector = "input[type=checkbox]";

    private readonly EditedViewModel viewModel = new();

    [SetUp]
    public void SetUp()

        // A binding control is a Fluxor component by inheritance, which is all this is here for.
        => this.Services.AddSingleton(new Mock<IActionSubscriber>().Object);

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [TestCase(true)]
    [TestCase(false)]
    public void Render__WhenTheViewModelHoldsATick__ThenShouldShowIt(bool givenTick)
    {
        // Given:
        this.viewModel.IsEnabled = givenTick;

        // When:
        IRenderedComponent<BindingCheckbox<EditedViewModel>> component = this.RenderCheckbox();

        // Then:
        Assert.That(
            component.Find(CheckboxSelector).HasAttribute("checked"), Is.EqualTo(givenTick));
    }

    [Test]
    public void Edit__WhenTheTesterTicksIt__ThenShouldWriteItToTheViewModel()
    {
        // Given:
        IRenderedComponent<BindingCheckbox<EditedViewModel>> component = this.RenderCheckbox();

        // When:
        component.Find(CheckboxSelector).Change(true);

        // Then:
        Assert.That(this.viewModel.IsEnabled, Is.True);
    }

    [Test]
    public void Edit__WhenTheTickHasBeenWritten__ThenShouldTellWhoeverAskedToHearOfIt()
    {
        // Given:
        bool? told = null;

        IRenderedComponent<BindingCheckbox<EditedViewModel>> component = this.RenderCheckbox(
            parameters => parameters.Add(checkbox => checkbox.OnChange, value => told = value));

        // When:
        component.Find(CheckboxSelector).Change(true);

        // Then:
        Assert.That(told, Is.True);
    }

    [Test]
    public void Edit__WhenTheBindingEventIsOnInput__ThenShouldWriteTheTickAsItIsMade()
    {
        // Given:
        IRenderedComponent<BindingCheckbox<EditedViewModel>> component = this.RenderCheckbox(
            parameters => parameters.Add(checkbox => checkbox.BindingEvent, BindingEvent.OnInput));

        // When:
        component.Find(CheckboxSelector).Input(true);

        // Then:
        Assert.That(this.viewModel.IsEnabled, Is.True);
    }

    private IRenderedComponent<BindingCheckbox<EditedViewModel>> RenderCheckbox(
        Action<ComponentParameterCollectionBuilder<BindingCheckbox<EditedViewModel>>>? added = null)
        => this.RenderComponent<BindingCheckbox<EditedViewModel>>(
            parameters =>
            {
                parameters
                    .AddCascadingValue(this.viewModel)
                    .Add(checkbox => checkbox.Binding, model => model.IsEnabled);

                added?.Invoke(parameters);
            });
}
