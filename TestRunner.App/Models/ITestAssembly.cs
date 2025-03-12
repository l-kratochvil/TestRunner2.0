namespace TestRunner.App.Models;

using System;

using TestRunner.Common;

internal interface ITestAssembly
{
    public TestSuiteEntity[] TestSuites { get; }
}