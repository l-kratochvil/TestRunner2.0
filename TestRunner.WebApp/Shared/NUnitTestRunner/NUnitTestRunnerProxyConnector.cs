namespace TestRunner.WebApp.Shared.NUnitTestRunner;

using System.Diagnostics;
using System.IO.Pipes;
using StreamJsonRpc;
using TestRunner.Common.Services;

/// <summary>
/// Owns the out-of-process NUnit proxy server: launches it, connects to it over a named pipe and
/// hands out the runner behind it. Disposing tears the connection down and stops the server.
/// </summary>
/// <remarks>
/// The server targets .NET Framework so that it can load <c>net481</c> test assemblies this
/// application cannot host, see the README next to the proxy project. It is therefore a process of
/// its own, and everything said to it travels over StreamJsonRpc.
/// </remarks>
internal sealed class NUnitTestRunnerProxyConnector : IAsyncDisposable
{
    /// <summary>
    /// Where the build leaves the server, relative to our own output directory.
    /// </summary>
    private const string ServerRelativePath =
        @"TestRunner.NUnitTestRunnerProxy\TestRunner.NUnitTestRunnerProxy.exe";

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
    /// Gets the runner living in the server process.
    /// </summary>
    public INUnitTestRunnerProxy Proxy { get; }

    /// <summary>
    /// Launches the proxy server and connects to it.
    /// </summary>
    /// <param name="cancellationToken">Token abandoning the attempt.</param>
    /// <returns>The connector owning the running server.</returns>
    /// <exception cref="FileNotFoundException">
    /// The server is missing from our output, which means the build did not copy it.
    /// </exception>
    public static async Task<NUnitTestRunnerProxyConnector> ConnectAsync(
        CancellationToken cancellationToken = default)
    {
        string serverPath = Path.Combine(AppContext.BaseDirectory, ServerRelativePath);

        if (!File.Exists(serverPath))
        {
            throw new FileNotFoundException(
                $"The NUnit proxy server executable was not found: {serverPath}", serverPath);
        }

        // A pipe name of its own per connection lets several instances of the application run side
        // by side without one of them answering the other's server.
        string pipeName = $"TestRunnerProxy_{Guid.NewGuid():N}";

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
            using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, connectTimeout.Token);

            await pipe.WaitForConnectionAsync(linked.Token);

            var handler = new HeaderDelimitedMessageHandler(pipe, pipe, new SystemTextJsonFormatter());
            var rpc = new JsonRpc(handler);
            INUnitTestRunnerProxy proxy = rpc.Attach<INUnitTestRunnerProxy>();

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