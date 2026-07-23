namespace TestRunner.Common;

using TestRunner.Common.Model;

public record TestRunResult(
    TestStatus Status,
    IgnoredResult[] IgnoredResults,
    ErrorResult[] ErrorResults,
    FailureResult[] FailureResults)
{
    public IgnoredResult[] IgnoredResults { get; } = IgnoredResults;

    public FailureResult[] FailureResults { get; } = FailureResults;

    public ErrorResult[] ErrorResults { get; } = ErrorResults;

    public TestStatus Status { get; } = Status;
}

public record IgnoredResult(
    string EntityName,
    string Message,
    string StackTrace)
    : UnsuccessfulResult(EntityName, Message, StackTrace);

public record FailureResult(
    string EntityName,
    string Message,
    string StackTrace)
    : UnsuccessfulResult(EntityName, Message, StackTrace);

public record ErrorResult(
    string EntityName,
    string Message,
    string StackTrace)
    : UnsuccessfulResult(EntityName, Message, StackTrace);

public record UnsuccessfulResult(
    string EntityName,
    string Message,
    string StackTrace)
{
    public string EntityName { get; } = EntityName;

    public string Message { get; } = Message;

    public string StackTrace { get; } = StackTrace;
}