namespace Zat.Tests.Runner.WebApp.Shared.TestRunnerBridge;

/// <summary>
/// Represents a proxy for a connected resource. Handles both synchronous and asynchronous disposal of the resource.
/// </summary>
/// <typeparam name="TConnected">The type of the connected resource.</typeparam>
/// <param name="connected">The connected resource instance.</param>
public sealed class ConnectedProxy<TConnected>(
    TConnected connected)
    : IAsyncDisposable
{
    /// <summary>
    /// Gets the connected resource instance.
    /// </summary>
    public TConnected Connected { get; private set; } = connected;

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        (this.Connected as IDisposable)?.Dispose();

        if (this.Connected is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
    }
}