namespace TestRunner.WebApp.Tests.TestExecution;

using Bunit;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using TestRunner.WebApp.Shared.Stores;
using TestExecutionComponent = TestRunner.WebApp.Features.TestExecution.Components.TestExecution;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TestExecutionTests : Bunit.TestContext
{
    private const string ButtonSelector = "button";

    private const string StartLabel = "Start";
    private const string StopLabel = "Stop";

    [SetUp]
    public void SetUp()
    {
        // The component is handed the configuration a run will be started with once starting is
        // wired up; toggling itself never reads it, so a mock without behaviour is enough.
        this.Services.AddSingleton(new Mock<IState<TestConfigurationState>>().Object);
    }

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void Render__WhenNothingHasBeenClicked__ThenShouldOfferToStartTheRun()
    {
        // When:
        IRenderedComponent<TestExecutionComponent> component =
            this.RenderComponent<TestExecutionComponent>();

        // Then:
        Assert.That(Label(component), Is.EqualTo(StartLabel));
    }

    [TestCase(1, StopLabel)]
    [TestCase(2, StartLabel)]
    public void OnToggle__WhenTheButtonIsClicked__ThenShouldOfferTheOppositeAction(
        int givenClicks, string expectedLabel)
    {
        // Given:
        IRenderedComponent<TestExecutionComponent> component =
            this.RenderComponent<TestExecutionComponent>();

        // When:
        for (var click = 0; click < givenClicks; click++)
        {
            component.Find(ButtonSelector).Click();
        }

        // Then:
        Assert.That(Label(component), Is.EqualTo(expectedLabel));
    }

    private static string Label(IRenderedComponent<TestExecutionComponent> component)
        => component.Find(ButtonSelector).TextContent.Trim();
}