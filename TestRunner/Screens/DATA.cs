using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TestRunner.Interfaces;

namespace TestRunner.Screens
{
    // TODO: Remove after loading real test entties data

    // TODO: Fetch from TestLink

    internal static class DATA
    {
        public static TestSuiteEntity[] TestSuites =>
        [
            new TestSuiteEntity(1, "TESTSUITE - A", TestCases),
            new TestSuiteEntity(2, "TESTSUITE - B", []),
            new TestSuiteEntity(3, "TESTSUITE - C", []),
            new TestSuiteEntity(4, "TESTSUITE - D", []),
            new TestSuiteEntity(5, "TESTSUITE - E", []),
        ];

        public static TestCaseEntity[] TestCases { get; } =
        [
            new TestCaseEntity(1, "Z200_170"),
            new TestCaseEntity(2, "Z200_171"),
            new TestCaseEntity(3, "Z200_180"),
            new TestCaseEntity(4, "Z200_90"),
            new TestCaseEntity(5, "Z200_87"),
        ];
    }
}