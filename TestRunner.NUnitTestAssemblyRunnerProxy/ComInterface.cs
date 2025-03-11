namespace TestRunner.App.NUnitTestAssemblyRunnerProxy;

using System.Runtime.InteropServices;

using App.Common.COM;

[ComVisible(true)]
[Guid(TestRunner.App.Common.COM.Constants.ClassGuid)]
[ClassInterface(ClassInterfaceType.None)]
// TODO: What's the function of this attribute?
// TODO: How to set namespace not by literal?
[ProgId("TestRunner.NUnitTestAssemblyRunnerProxy." + nameof(ComInterface))]
public class ComInterface : TestRunner.App.Common.COM.INUnitTestRunner
{
    public void LoadTestEntities()
    {
        throw new NotImplementedException();
    }

    public void RunTestAsync()
    {
        throw new NotImplementedException();
    }

    public void StopTest(bool force)
    {
        throw new NotImplementedException();
    }
}