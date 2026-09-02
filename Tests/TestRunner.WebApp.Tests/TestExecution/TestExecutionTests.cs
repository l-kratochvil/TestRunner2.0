namespace TestRunner.WebApp.Tests.TestExecution;

using Bunit;

using Fluxor;

using Microsoft.Extensions.DependencyInjection;

using Moq;
using NUnit.Framework;

using TestRunner.Common.Model;
using TestRunner.WebApp.Features.TestConfiguration.Services;
using TestRunner.WebApp.Shared.Domain;
using TestRunner.WebApp.Shared.Stores;
using TestExecutionComponent = TestRunner.WebApp.Features.TestExecution.Components.TestExecution;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TestExecutionTests : Bunit.TestContext
{
    private const string ButtonSelector = "button";

    private const string StartLabel = "Start";
    private const string StopLabel = "Stop";

    private TestConfigurationState configuration = new();
    private TestDiscoveryState testSelection = new([]);

    [SetUp]
    public void SetUp()
    {
        var configurationState = new Mock<IState<TestConfigurationState>>();
        configurationState.SetupGet(state => state.Value).Returns(() => this.configuration);

        var testDiscoveryStore = new Mock<ITestDiscoveryStore>();
        testDiscoveryStore.SetupGet(store => store.Current).Returns(() => this.testSelection);

        this.Services.AddSingleton(configurationState.Object);
        this.Services.AddSingleton(testDiscoveryStore.Object);

        // The real rules rather than a stand-in: what the button refuses to do is exactly what they
        // say, so a stand-in would test the wiring against itself.
        this.Services.AddSingleton<ITestConfigurationValidator, TestConfigurationValidator>();
    }

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void Render__WhenNothingHasBeenClicked__ThenShouldOfferToStartTheRun()
    {
        // Given:
        this.GivenARunnableConfiguration();

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
        this.GivenARunnableConfiguration();

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

    [Test]
    public void Render__WhenNoTestIsSelected__ThenShouldNotLetTheRunStart()
    {
        // Given:
        // A run of nothing is not a run, and the reason sits on the button because that is what the
        // tester is looking at rather than the explorer beside it.
        this.configuration = RunnableConfiguration();
        this.testSelection = new TestDiscoveryState([]);

        // When:
        IRenderedComponent<TestExecutionComponent> component =
            this.RenderComponent<TestExecutionComponent>();

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(IsDisabled(component), Is.True);
            Assert.That(Reason(component), Does.Contain("Select the tests"));
        });
    }

    [Test]
    public void Render__WhenTheConfigurationIsNotFinished__ThenShouldNotLetTheRunStart()
    {
        // Given:
        this.configuration = new TestConfigurationState();
        this.GivenSelectedTestCase(TestType.ApplicationTest);

        // When:
        IRenderedComponent<TestExecutionComponent> component =
            this.RenderComponent<TestExecutionComponent>();

        // Then:
        Assert.Multiple(() =>
        {
            Assert.That(IsDisabled(component), Is.True);
            Assert.That(Reason(component), Is.Not.Empty);
        });
    }

    [Test]
    public void Render__WhenARuntimeTestIsSelectedWithoutAStation__ThenShouldNotLetTheRunStart()
    {
        // Given:
        // Which station a runtime test runs on is part of what is tested, and the selection is the
        // only thing that knows a runtime test is in it.
        this.configuration = RunnableConfiguration();
        this.GivenSelectedTestCase(TestType.RuntimeTest);

        // When:
        IRenderedComponent<TestExecutionComponent> component =
            this.RenderComponent<TestExecutionComponent>();

        // Then:
        Assert.That(IsDisabled(component), Is.True);
    }

    [Test]
    public void Render__WhenARuntimeTestIsSelectedWithAStation__ThenShouldLetTheRunStart()
    {
        // Given:
        this.configuration = RunnableConfiguration() with
        {
            TestedHwAssembly = TestedHwAssemblyType.HW01,
        };
        this.GivenSelectedTestCase(TestType.RuntimeTest);

        // When:
        IRenderedComponent<TestExecutionComponent> component =
            this.RenderComponent<TestExecutionComponent>();

        // Then:
        Assert.That(IsDisabled(component), Is.False);
    }

    private static TestConfigurationState RunnableConfiguration()
        => new(
            IsTestLinkEnabled: false,
            IdeVersion: null,
            RuntimeVersion: "6",
            TestedHwAssembly: null);

    private static string Label(IRenderedComponent<TestExecutionComponent> component)
        => component.Find(ButtonSelector).TextContent.Trim();

    private static bool IsDisabled(IRenderedComponent<TestExecutionComponent> component)
        => component.Find(ButtonSelector).HasAttribute("disabled");

    private static string? Reason(IRenderedComponent<TestExecutionComponent> component)
        => component.Find(ButtonSelector).GetAttribute("title");

    private void GivenARunnableConfiguration()
    {
        this.configuration = RunnableConfiguration();
        this.GivenSelectedTestCase(TestType.ApplicationTest);
    }

    private void GivenSelectedTestCase(TestType testType)
        => this.testSelection = new TestDiscoveryState(
            [new TestCaseEntity(testType, id: "1", name: "Test", executionPath: "Suite.Fixture.Test")]);
}
