using System;

namespace TestRunner.App.Models;

internal interface ITestAssembly
{
    public TestSuiteEntity[] TestSuites { get; }
}