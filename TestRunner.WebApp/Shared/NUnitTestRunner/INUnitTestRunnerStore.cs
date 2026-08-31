namespace TestRunner.WebApp.Shared.NUnitTestRunner;

using TestRunner.Common.Model;

/// <summary>
/// The test tree the application discovered on start, as the rest of the application reads it.
/// </summary>
/// <remarks>
/// Discovery happens once per run of the application rather than per browser tab, because the test
/// assemblies belong to the test machine and not to whoever is looking at them.
/// </remarks>
public interface INUnitTestRunnerStore
{
    /// <summary>
    /// Gets the test suites discovered from the test assemblies. Empty when nothing was
    /// discovered, including when discovery failed.
    /// </summary>
    IReadOnlyList<TestSuiteEntity> LoadedTestSuites { get; }
}