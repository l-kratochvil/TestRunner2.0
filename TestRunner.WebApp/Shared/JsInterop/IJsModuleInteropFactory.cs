namespace TestRunner.WebApp.Shared.JsInterop;

/// <summary>
/// Creates the wrappers around JavaScript modules.
/// </summary>
/// <remarks>
/// Components ask the factory instead of constructing a <see cref="JsModuleInterop"/> themselves,
/// so that the JavaScript runtime and the logger are wired up in one place and a component only
/// has to name its module.
/// </remarks>
public interface IJsModuleInteropFactory
{
    /// <summary>
    /// Creates a wrapper around the module at the given path.
    /// </summary>
    /// <param name="modulePath">
    /// Path of the module: either a collocated script relative to the web root, for example
    /// <c>./Components/Layout/SplitterBar.razor.js</c>, or the URL a shared module is served
    /// under, for example <c>/js/logging.js</c>.
    /// </param>
    /// <returns>The wrapper, to be disposed by the caller that asked for it.</returns>
    JsModuleInterop Create(string modulePath);
}