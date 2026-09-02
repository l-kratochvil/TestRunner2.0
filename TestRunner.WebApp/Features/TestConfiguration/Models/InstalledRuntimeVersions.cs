namespace TestRunner.WebApp.Features.TestConfiguration.Models;

using System.Linq;


/// <summary>
/// The runtime versions installed on the test machine, as the configurator offers them.
/// </summary>
/// <param name="Versions">
/// Versions found, newest first. Empty when there are none to offer, whatever the reason.
/// </param>
/// <param name="IsInstallFolderReadable">
/// Whether the folder the versions are installed in could be read at all. Distinguishes a folder
/// that is missing or out of reach — which the tester fixes in the settings — from one holding no
/// runtime version yet, which they fix by installing one.
/// </param>
public sealed record InstalledRuntimeVersions(
    IReadOnlyList<string> Versions,
    bool IsInstallFolderReadable)
{
    /// <summary>
    /// Says whether a version is one of those found installed.
    /// </summary>
    /// <remarks>
    /// The one place that decides it, because a version that is chosen but not installed has to be
    /// noticed the same way wherever it turns up: put back from the browser, or left behind when
    /// the install folder was pointed somewhere else.
    /// </remarks>
    /// <param name="version">Version to look for, if any.</param>
    /// <returns><see langword="true"/> when it is installed.</returns>
    public bool Includes(string? version)
        => !string.IsNullOrEmpty(version)
            && this.Versions.Contains(version, StringComparer.Ordinal);
}
