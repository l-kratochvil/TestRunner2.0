using System;

namespace TestRunner.Models;

internal interface ITestAssembly
{
    public TestSuiteEntity[] TestSuites { get; }
}
