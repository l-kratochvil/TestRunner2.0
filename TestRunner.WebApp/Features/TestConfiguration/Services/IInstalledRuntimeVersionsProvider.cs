namespace TestRunner.WebApp.Features.TestConfiguration.Services;

using TestRunner.WebApp.Features.TestConfiguration.Models;

/// <summary>
/// Answers which runtime versions are installed on the test machine.
/// </summary>
public interface IInstalledRuntimeVersionsProvider
{
    /// <summary>
    /// Reads the runtime versions installed under the folder named by the application settings.
    /// </summary>
    /// <remarks>
    /// Read on every call rather than remembered, because the tester may install a runtime while
    /// the application is running.
    /// </remarks>
    /// <returns>The versions found, and whether the folder could be read at all.</returns>
    InstalledRuntimeVersions Read();
}
