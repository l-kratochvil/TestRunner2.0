namespace TestRunner.App.COM;

using System.Runtime.InteropServices;

using TestRunner.Common;
using TestRunner.Common.COM;

[ComImport]
[Guid(Constants.COM.ClassGuid)]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
interface INUnitTestRunnerComClient : IComBridge
{
}