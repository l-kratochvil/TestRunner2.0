namespace Zat.Tests.Runner.WebApp.Shared.TestRunnerBridge;

using System.IO.Pipes;
using StreamJsonRpc;
using Zat.Z2xxTests.Common.Model;
using Zat.Z2xxTests.Common.Services;

public class TestRunnerBridgeConnector : ITestRunnerBridgeConnector
{
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(60);

    /// <inheritdoc/>
    public async Task ConnectAsync(
        TestConfig testConfig,
        CancellationToken cancellationToken = default)
    {
        // Constructing the stream reserves the pipe name, so a client starting right after this
        // call returns already finds the pipe, even before the connection is accepted below.
        await using var pipe = new NamedPipeServerStream(
            TestRunnerBridgeConfig.PipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);

        using var connectTimeout = new CancellationTokenSource(ConnectTimeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, connectTimeout.Token);

        await pipe.WaitForConnectionAsync(linked.Token).ConfigureAwait(false);

        var formatter = new SystemTextJsonFormatter();
        var handler = new HeaderDelimitedMessageHandler(pipe, pipe, formatter);

        using var rpc = new JsonRpc(handler);
        rpc.AddLocalRpcTarget<ITestRunnerBridge>(
            new TestRunnerBridge(testConfig),
            new JsonRpcTargetOptions { DisposeOnDisconnect = true });

        rpc.StartListening();

        using var cancellation = cancellationToken.Register(rpc.Dispose);

        try
        {
            await rpc.Completion.ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is ConnectionLostException or OperationCanceledException)
        {
            // The client closing the pipe, or the run being cancelled, is how this session ends.
        }
    }
}