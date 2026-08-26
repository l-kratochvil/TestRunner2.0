namespace TestRunner.WebApp.Tests.Components.Layout;

using Bunit;
using Microsoft.JSInterop;
using NUnit.Framework;
using TestRunner.WebApp.Components.Layout;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class SplitterBarTests : Bunit.TestContext
{
    private const string ModulePath = "./Components/Layout/SplitterBar.razor.js";

    [TearDown]
    public void TearDown()
        => this.Dispose();

    [Test]
    public void OnAfterRenderAsync__WhenTheComponentIsFirstRendered__ThenShouldInitializeTheJsModuleWithTheHandleAndOptions()
    {
        // Given:
        BunitJSModuleInterop module = this.JSInterop.SetupModule(ModulePath);
        JSRuntimeInvocationHandler initialize = module.SetupVoid("initialize", _ => true).SetVoidResult();

        // When:
        IRenderedComponent<SplitterBar> component = this.RenderComponent<SplitterBar>(parameters => parameters
            .Add(p => p.CssVariable, "--app-log-height")
            .Add(p => p.StorageKey, "log-height")
            .Add(p => p.MinSize, 100)
            .Add(p => p.MaxSizeRatio, 0.6)
            .Add(p => p.DefaultSizeRatio, 0.3));

        // Then:
        Assert.That(initialize.Invocations["initialize"], Has.Count.EqualTo(1));
        JSRuntimeInvocation invocation = initialize.Invocations["initialize"][0];
        invocation.Arguments[0].ShouldBeElementReferenceTo(component.Find(".splitter-bar"));

        object options = invocation.Arguments[1]!;
        Assert.Multiple(() =>
        {
            Assert.That(GetProperty(options, "CssVariable"), Is.EqualTo("--app-log-height"));
            Assert.That(GetProperty(options, "StorageKey"), Is.EqualTo("log-height"));
            Assert.That(GetProperty(options, "MinSize"), Is.EqualTo(100));
            Assert.That(GetProperty(options, "MaxSizeRatio"), Is.EqualTo(0.6));
            Assert.That(GetProperty(options, "DefaultSizeRatio"), Is.EqualTo(0.3));
        });
    }

    [Test]
    public void OnAfterRenderAsync__WhenTheComponentIsRerendered__ThenShouldInitializeTheJsModuleOnlyOnce()
    {
        // Given:
        BunitJSModuleInterop module = this.JSInterop.SetupModule(ModulePath);
        JSRuntimeInvocationHandler initialize = module.SetupVoid("initialize", _ => true).SetVoidResult();
        IRenderedComponent<SplitterBar> component = this.RenderComponent<SplitterBar>(parameters => parameters
            .Add(p => p.CssVariable, "--app-log-height")
            .Add(p => p.StorageKey, "log-height"));

        // When:
        component.SetParametersAndRender(parameters => parameters.Add(p => p.Label, "Different label"));

        // Then:
        Assert.That(initialize.Invocations["initialize"], Has.Count.EqualTo(1));
    }

    [Test]
    public async Task DisposeAsync__WhenTheComponentWasInitialized__ThenShouldInvokeTheJsDisposeWithTheHandle()
    {
        // Given:
        BunitJSModuleInterop module = this.JSInterop.SetupModule(ModulePath);
        module.SetupVoid("initialize", _ => true).SetVoidResult();
        JSRuntimeInvocationHandler dispose = module.SetupVoid("dispose", _ => true).SetVoidResult();
        IRenderedComponent<SplitterBar> component = this.RenderComponent<SplitterBar>(parameters => parameters
            .Add(p => p.CssVariable, "--app-log-height")
            .Add(p => p.StorageKey, "log-height"));

        // When:
        await component.Instance.DisposeAsync();

        // Then:
        Assert.That(dispose.Invocations["dispose"], Has.Count.EqualTo(1));
        dispose.Invocations["dispose"][0].Arguments[0].ShouldBeElementReferenceTo(component.Find(".splitter-bar"));
    }

    [Test]
    public void DisposeAsync__WhenTheComponentWasNeverRendered__ThenShouldNotThrow()
    {
        // Given:
        SplitterBar unit = new();

        // When / Then:
        Assert.That(async () => await unit.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public void DisposeAsync__WhenTheJsDisposeCallThrowsBecauseTheBrowserDisconnected__ThenShouldSwallowTheException()
    {
        // Given:
        BunitJSModuleInterop module = this.JSInterop.SetupModule(ModulePath);
        module.SetupVoid("initialize", _ => true).SetVoidResult();
        module.SetupVoid("dispose", _ => true).SetException(new JSDisconnectedException("The circuit disconnected."));
        IRenderedComponent<SplitterBar> component = this.RenderComponent<SplitterBar>(parameters => parameters
            .Add(p => p.CssVariable, "--app-log-height")
            .Add(p => p.StorageKey, "log-height"));

        // When / Then:
        Assert.That(async () => await component.Instance.DisposeAsync(), Throws.Nothing);
    }

    private static object? GetProperty(object source, string name)
        => source.GetType().GetProperty(name)!.GetValue(source);
}
