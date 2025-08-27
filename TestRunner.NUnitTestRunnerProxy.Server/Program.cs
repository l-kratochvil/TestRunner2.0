using System;
using System.Runtime.InteropServices;
using System.Threading;
using TestRunner.NUnitTestRunnerProxy;

internal static class Program
{
    private static ManualResetEventSlim s_quit = new(initialState: false);

    [STAThread]
    private static int Main()
    {
        // Register COM class factory for out-of-proc activation
        var regSrv = new RegistrationServices();
        int cookie = 0;
        try
        {
            cookie = regSrv.RegisterTypeForComClients(
                typeof(NUnitTestRunnerProxy),
                RegistrationClassContext.LocalServer,
                RegistrationConnectionType.MultipleUse);

            // Keep the server alive until the COM runtime disconnects all clients
            Console.WriteLine("COM local server registered. Waiting for clients...");
            s_quit.Wait();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Server failed: {ex}");
            return Marshal.GetHRForException(ex);
        }
        finally
        {
            if (cookie != 0)
            {
                try { regSrv.UnregisterTypeForComClients(cookie); } catch { }
            }
        }
    }
}
