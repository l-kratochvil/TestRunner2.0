This folder is reserved for an optional out-of-proc COM server host if you decide to support .NET 8 client calling a .NET Framework COM server via LocalServer32 (EXE) or COM+ surrogate. In-proc reg-free COM with a .NET Framework server cannot be loaded into a .NET 8 process due to CLR version constraints.

Options:

- Build an out-of-proc (EXE) COM server hosting your .NET Framework logic and reference it via LocalServer32 in a reg-free manifest.
- Alternatively, expose the .NET Framework class via COM+ (dllhost.exe surrogate) and use a reg-free proxy/stub.
- Or, port the COM server to .NET 8 and use ComWrappers / ComVisible with a .comhost.
