namespace TestRunner.Common;

using ComplexTypes;

public record TestResult(TestStatus Status)
{
    public TestStatus Status { get; } = Status;
}