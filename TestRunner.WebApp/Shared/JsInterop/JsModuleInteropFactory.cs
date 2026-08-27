namespace TestRunner.WebApp.Shared.JsInterop;

using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

/// <summary>
/// Creates <see cref="JsModuleInterop"/> instances bound to the circuit of the caller.
/// </summary>
/// <remarks>
/// <para>
/// Registered as scoped, because <see cref="IJSRuntime"/> is. In Blazor Server a scope is one
/// circuit, that is one open browser tab from the moment it connects until it goes away, and the
/// runtime resolved within it is the one talking to that tab. Components are resolved from the
/// same scope, so the factory is handed exactly the runtime the component would have injected
/// itself.
/// </para>
/// <para>
/// It must not be a singleton: a singleton would capture the runtime of whichever circuit happened
/// to build it first, and every later tab would then be calling into a foreign, eventually dead
/// browser session. Scope validation cannot catch that, because nothing about the object graph is
/// wrong; only the lifetime is.
/// </para>
/// </remarks>
/// <param name="js">JavaScript runtime of the circuit this factory was resolved in.</param>
/// <param name="logger">Logger handed to every created wrapper.</param>
public sealed class JsModuleInteropFactory(
    IJSRuntime js,
    ILogger<JsModuleInterop> logger) : IJsModuleInteropFactory
{
    /// <inheritdoc/>
    public JsModuleInterop Create(string modulePath)
        => new(js, modulePath, logger);
}