namespace TestRunner.Common;

using ComplexTypes;

public class SimpleTypes
{
    public record TestResult(TestStatus Status)
    {
        public TestStatus Status { get; } = Status;
    }
}