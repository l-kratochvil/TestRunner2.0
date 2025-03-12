using System.Threading.Tasks;

namespace TestRunner.Common;

public class Types
{
    /// <summary>
    /// The TestStatus enum indicates the result of running a test
    /// </summary>
    public enum TestStatus
    {
        /// <summary>The test was inconclusive</summary>
        Inconclusive,

        /// <summary>The test has been skipped</summary>
        Skipped,

        /// <summary>The test succeeded</summary>
        Passed,

        /// <summary>There was a warning</summary>
        Warning,

        /// <summary>The test failed</summary>
        Failed,
    }

    public record TestResult(TestStatus Status)
    {
        public TestStatus Status { get; } = Status;
    }
}