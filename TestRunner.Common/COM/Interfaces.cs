namespace TestRunner.Common.COM;

using System.Runtime.InteropServices;

/// <summary>
/// Describes NUnit test runner proxy surface COM object.
/// </summary>
[Guid(Guids.NUnitTestRunnerProxyInterfaceGuid)]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
[ComVisible(true)]
[ComImport]
public interface INUnitTestRunnerProxy
{
    bool IsAssemblyLoaded { get; }

    bool IsTestRunning { get; }

    void StopTest(bool force);

    ITestSuiteEntity[] GetTestSuiteEntities(string dllPath);
}

/// <summary>
/// Describes test suite entity COM object.
/// </summary>
[Guid("4f6312e1-252a-4749-8f7a-3971761e5db8")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
[ComVisible(true)]
[ComImport]
public interface ITestSuiteEntity
{
}