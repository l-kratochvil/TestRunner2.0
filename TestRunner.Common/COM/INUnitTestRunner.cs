using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TestRunner.App.Common.COM;

[ComVisible(true)]
[Guid(Constants.InterfaceGuid)] // Vygenerujte vlastní GUID
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface INUnitTestRunner
{
    // TODO: Make this async
    void RunTestAsync( /* TODO: IEnumerable<TestSuiteEntity> testsuites, string dllPath*/);

    void StopTest(bool force);

    // TODO: Return TestSuiteEntity[]
    public void LoadTestEntities();
}