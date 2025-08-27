namespace TestRunner.App.COM;

using System.Runtime.InteropServices;

using TestRunner.Common.COM;

[ComImport]
[Guid(Guids.NUnitTestRunnerProxyInterfaceGuid)]
[CoClass(typeof(NUnitTestRunnerProxy))]
public interface INUnitTestRunnerProxy : TestRunner.Common.COM.INUnitTestRunnerProxy
{
}