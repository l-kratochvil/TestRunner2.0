namespace TestRunner.App.Models;

using TestRunner.Common.ComplexTypes;

internal interface ITestAssembly
{
    public TestSuiteEntity[] TestSuites { get; }
}