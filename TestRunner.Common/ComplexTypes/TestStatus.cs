using System;
using System.Collections.Generic;
using System.Text;

namespace TestRunner.Common.ComplexTypes;

public enum TestStatus
{
    Unknown,

    /// <summary>The test was inconclusive</summary>
    Inconclusive,

    /// <summary>The test has skipped</summary>
    Skipped,

    /// <summary>The test succeeded</summary>
    Passed,

    /// <summary>There was a warning</summary>
    Warning,

    /// <summary>The test failed</summary>
    Failed,
}