namespace TestRunner.Common;

using TestRunner.Common.ComplexTypes;

public record TestResult(TestStatus Status)
{
    public TestStatus Status { get; } = Status;
}