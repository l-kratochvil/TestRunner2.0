namespace TestRunner.App.COM;

using System.Runtime.InteropServices;

using TestRunner.Common;

[ComImport]
[Guid(Constants.COM.ClassGuid)]
[ClassInterface(ClassInterfaceType.None)]
// TODO: What's the function of this attribute?
// TODO: How to set namespace not by literal?
[ProgId("TestRunner.App." + nameof(NUnitTestRunnerComClient))]
public class NUnitTestRunnerComClient
{
}