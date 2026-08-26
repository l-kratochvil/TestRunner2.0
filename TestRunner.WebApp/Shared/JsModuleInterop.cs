namespace TestRunner.WebApp.Shared;

using Microsoft.JSInterop;

/// <summary>
/// Wraps calls into a collocated .razor.js module so the module path is spelled out in exactly one
/// place. Owned and disposed by the component that creates it (not DI-registered), following the
/// pattern in https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability/.
/// </summary>
public sealed class JsModuleInterop(
    IJSRuntime js,
    string modulePath) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask = new(
        () => js.InvokeAsync<IJSObjectReference>("import", modulePath).AsTask());

    /// <summary>
    /// Calls an exported function of the module that returns nothing.
    /// </summary>
    public async Task InvokeVoidAsync(string identifier, params object?[]? args)
        => await (await this.moduleTask.Value).InvokeVoidAsync(identifier, args);

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (!this.moduleTask.IsValueCreated)
        {
            return;
        }

        try
        {
            IJSObjectReference module = await this.moduleTask.Value;
            await module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // The browser is already gone; nothing to clean up on the client.
        }
    }
}
