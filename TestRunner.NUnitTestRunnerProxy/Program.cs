using System.IO.Pipes;

using StreamJsonRpc;

using TestRunner.NUnitTestRunnerProxy;

internal static class Program
{
    /// <summary>
    /// Entry point of the out-of-process NUnit runner. The application launches this process and passes the
    /// name of a named pipe it is already listening on; this process connects as the pipe client and serves
    /// JSON-RPC requests until the application closes the connection.
    /// </summary>
    private static async Task<int> Main(string[] args)
    {
        if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
        {
            Console.Error.WriteLine("Usage: TestRunner.NUnitTestRunnerProxy.Server <pipe-name>");
            return 1;
        }

        var pipeName = args[0];

        try
        {
            using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await pipe.ConnectAsync((int)TimeSpan.FromSeconds(15).TotalMilliseconds).ConfigureAwait(false);

            var formatter = new SystemTextJsonFormatter();
            var handler = new HeaderDelimitedMessageHandler(pipe, pipe, formatter);

            using var rpc = new JsonRpc(handler);
            rpc.AddLocalRpcTarget(new NUnitTestRunnerProxy(), new JsonRpcTargetOptions { DisposeOnDisconnect = true });
            rpc.StartListening();

            await rpc.Completion.ConfigureAwait(false);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Server failed: {ex}");
            return 1;
        }
    }
}
