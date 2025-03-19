namespace TestRunner.NUnitTestRunnerProxy;

using System.Collections.Generic;
using System.Runtime.InteropServices;

using TestRunner.Common;
using TestRunner.Common.ComplexTypes;

[ComVisible(true)]
[Guid(Constants.COM.ClassGuid)]
[ClassInterface(ClassInterfaceType.None)]
// TODO: What's the function of this attribute?
// TODO: How to set namespace not by literal?
[ProgId("TestRunner.NUnitTestAssemblyRunnerProxy." + nameof(ComServer))]
public class ComServer : IComServer
{
    private readonly TestRunnerEngine runner = new();

    public bool IsAssemblyLoaded => throw new NotImplementedException();

    public bool IsTestRunning => throw new NotImplementedException();

    public void RunTestAsync()
    {
        throw new NotImplementedException();
    }

    public void StopTest(bool force)
    {
        throw new NotImplementedException();
    }

    public SimpleTypes.TestResult[] RunTest(IEnumerable<TestSuiteEntity> testEntitiesToRun, string dllPath)
    {
        throw new NotImplementedException();
    }

    public TestSuiteEntity[] GetTestSuiteEntities(string dllPath)
    {
        throw new NotImplementedException();
    }
}

[ComVisible(true)]
[Guid(Constants.COM.ClassGuid)]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
internal interface IComServer : Common.COM.IComBridge
{
}