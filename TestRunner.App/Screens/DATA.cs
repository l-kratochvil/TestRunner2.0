namespace TestRunner.App.Screens;

using TestRunner.Common.Model;

// TODO: Remove after loading real test entties data

// TODO: Fetch from TestLink

internal static class DATA
{
    public static TestSuiteEntity[] TestSuites =>
    [
        new(TestCases, TestType.ApplicationTest, "TESTSUITE - A", "Path1"),
        new([], TestType.ApplicationTest, "TESTSUITE - B", "Path2"),
        new([], TestType.ApplicationTest, "TESTSUITE - C", "Path3"),
        new([], TestType.ApplicationTest, "TESTSUITE - D", "Path4"),
        new([], TestType.ApplicationTest, "TESTSUITE - E", "Path5"),
    ];

    public static TestCaseEntity[] TestCases { get; } =
    [
        new(TestType.RuntimeTest, "Z200_170", "Path"),
        new(TestType.RuntimeTest, "Z200_171", "Path"),
        new(TestType.RuntimeTest, "Z200_180", "Path"),
        new(TestType.RuntimeTest, "Z200_90", "Path"),
        new(TestType.RuntimeTest, "Z200_87", "Path"),
    ];
}