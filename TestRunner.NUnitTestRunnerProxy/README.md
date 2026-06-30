# TestRunner.NUnitTestRunnerProxy.Server

Out-of-process NUnit test runner that targets **.NET Framework 4.8.1**, so the NUnit engine can load
`net481` test assemblies that the main `net10.0` application cannot host in-process.

## How it works

- The application (`TestRunner.App`) creates a uniquely named pipe, launches this executable and passes the
  pipe name as the first command-line argument.
- This process connects to the pipe as the client and serves a
  [StreamJsonRpc](https://github.com/microsoft/vs-streamjsonrpc) endpoint backed by
  `SystemTextJsonFormatter`.
- The RPC surface is `TestRunner.Common.Services.INUnitTestRunnerProxy`, implemented by `NUnitTestRunnerProxy`.
- The server stays alive until the application closes the connection (`JsonRpc.Completion`), and is also killed
  by the application on exit as a fallback.

The build copies this project's output into the application's output directory (see the Exchange targets in
`Directory.Build.*`) so the application can launch it.
