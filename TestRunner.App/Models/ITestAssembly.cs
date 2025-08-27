namespace TestRunner.App.Models;

using TestRunner.Common.COM;

internal interface ITestAssembly
{
    public ITestSuiteEntity[] TestSuites { get; }
}