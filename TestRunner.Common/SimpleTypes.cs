namespace TestRunner.Common;

using TestRunner.Common.Model;

public record TestResult(TestStatus Status)
{
    public TestStatus Status { get; } = Status;
}