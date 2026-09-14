# C# rules

## Contents

- [Options are immutable](#options-are-immutable)
- [ViewModel suffix names a component's view model](#viewmodel-suffix-names-a-components-view-model)

## Options are immutable

An Options type exposes its values as `{ get; init; }`, so a service's configuration cannot change
underneath it. The instance is what is immutable — `IOptionsMonitor<T>` may still hand out a new one
on reload.

The binder sets `init` properties by reflection, so configuration binding works as before; mutating
an existing instance does not. Configure through `IConfiguration`, in tests too, instead of
`services.Configure<T>(options => …)`. A positional `record` cannot bind — the binder needs a
parameterless constructor.

## ViewModel suffix names a component's view model

The `ViewModel` suffix is reserved for the view model of a Razor component of the same base name —
`Component.razor` is the view, `ComponentViewModel` is its view model. For example,
[TestExplorer.razor](/TestRunner.WebApp/Features/TestDiscovery/Components/TestExplorer.razor) pairs
with [TestExplorerViewModel](/TestRunner.WebApp/Features/TestDiscovery/Components/TestExplorerViewModel.cs).

Other classes that hold UI state or data — a tree node, a row, a DTO shown on screen — keep their
own descriptive name instead of picking up the suffix (e.g. `TestTreeNodeData`, not
`TestTreeNodeViewModel`). That way `ViewModel` unambiguously means "the view model paired with this
view" wherever it appears, rather than "some class with UI-related data".
