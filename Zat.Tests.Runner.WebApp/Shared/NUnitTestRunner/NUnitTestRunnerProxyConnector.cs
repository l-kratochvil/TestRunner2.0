namespace Zat.Tests.Runner.WebApp.Shared.NUnitTestRunner;

using System.Diagnostics;
using System.IO.Pipes;

using StreamJsonRpc;

using Zat.Tests.Runner.Common.Services;

/// <summary>
/// Owns the out-of-process NUnit proxy server: launches it, connects to it over a named pipe, and
/// exposes <see cref="Proxy"/>. Disposing it closes the connection and stops the server.
/// </summary>
/// <remarks>
/// The server runs out of process because it targets .NET Framework and can load <c>net481</c>
/// test assemblies this application cannot host. Calls to <see cref="Proxy"/> cross that boundary
/// through <see cref="JsonRpc"/>.
/// </remarks>
internal sealed class NUnitTestRunnerProxyConnector : IAsyncDisposable
{
    /// <summary>
    /// Path of the server executable relative to this application's output directory.
    /// </summary>
    private const string ServerRelativePath =
        @"Zat.Tests.Runner.NUnitTestRunnerProxy\Zat.Tests.Runner.NUnitTestRunnerProxy.exe";

    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(60);

    private readonly Process serverProcess;
    private readonly NamedPipeServerStream pipe;
    private readonly JsonRpc rpc;

    private NUnitTestRunnerProxyConnector(
        Process serverProcess,
        NamedPipeServerStream pipe,
        JsonRpc rpc,
        INUnitTestRunnerProxy proxy)
    {
        this.serverProcess = serverProcess;
        this.pipe = pipe;
        this.rpc = rpc;
        this.Proxy = proxy;
    }

    /// <summary>
    /// Gets the test runner proxy served by the connected process.
    /// </summary>
    public INUnitTestRunnerProxy Proxy { get; }

    /// <summary>
    /// Launches the proxy server and connects to it.
    /// </summary>
    /// <param name="cancellationToken">Token abandoning the attempt.</param>
    /// <returns>The connected connector.</returns>
    /// <exception cref="FileNotFoundException">
    /// The server executable is missing from the output, so the build did not copy it.
    /// </exception>
    public static async Task<NUnitTestRunnerProxyConnector> ConnectAsync(
        CancellationToken cancellationToken = default)
    {
        var serverPath = Path.Combine(AppContext.BaseDirectory, ServerRelativePath);

        if (!File.Exists(serverPath))
        {
            throw new FileNotFoundException(
                $"The NUnit proxy server executable was not found: {serverPath}", serverPath);
        }

        // A pipe name of its own per connection lets several instances of the application run side
        // by side without one of them answering the other's server.
        var pipeName = $"Zat.Tests.RunnerProxy_{Guid.NewGuid():N}";

        var pipe = new NamedPipeServerStream(
            pipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);

        Process? serverProcess = null;

        try
        {
            serverProcess = StartServer(serverPath, pipeName);

            using var connectTimeout = new CancellationTokenSource(ConnectTimeout);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, connectTimeout.Token);

            await pipe.WaitForConnectionAsync(linked.Token);

            var handler = new HeaderDelimitedMessageHandler(pipe, pipe, new SystemTextJsonFormatter());
            var rpc = new JsonRpc(handler);
            var proxy = rpc.Attach<INUnitTestRunnerProxy>();

            rpc.StartListening();

            return new NUnitTestRunnerProxyConnector(serverProcess, pipe, rpc, proxy);
        }
        catch
        {
            await pipe.DisposeAsync();
            StopServer(serverProcess);
            serverProcess?.Dispose();

            throw;
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        // Closing the connection first lets the server observe the disconnect and leave on its own
        // terms; killing it is the fallback for a server that did not take the hint.
        this.rpc.Dispose();

        await this.pipe.DisposeAsync();

        StopServer(this.serverProcess);
        this.serverProcess.Dispose();
    }

    private static Process StartServer(string serverPath, string pipeName)
        => Process.Start(new ProcessStartInfo(serverPath, pipeName)
           {
               UseShellExecute = false,
               CreateNoWindow = true,
               WorkingDirectory = Path.GetDirectoryName(serverPath)!,
           })
           ?? throw new InvalidOperationException(
               $"The NUnit proxy server process could not be started: {serverPath}");

    private static void StopServer(Process? serverProcess)
    {
        try
        {
            if (serverProcess is { HasExited: false })
            {
                serverProcess.Kill();
            }
        }
        catch (Exception exception) when (exception
                                              is InvalidOperationException
                                                 or System.ComponentModel.Win32Exception
                                                 or NotSupportedException)
        {
            // A server that is already gone, or that refuses to be killed, must not keep the
            // application from shutting down: there is nothing left to do about it either way.
        }
    }
}