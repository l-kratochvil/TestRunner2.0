namespace TestRunner.WebApp.Features.TestConfiguration.Services;

using System.Linq;
using System.Text.RegularExpressions;

using TestRunner.WebApp.Features.TestConfiguration.Models;
using TestRunner.WebApp.Shared.Logging;
using TestRunner.WebApp.Shared.Storage;
using TestRunner.WebApp.Shared.Stores;

/// <summary>
/// Reads the installed runtime versions from the folders the IDE installer creates, one per
/// version, under the install folder named by the application settings.
/// </summary>
/// <remarks>
/// A runtime version is the name of its folder rather than a number, because that name is what
/// identifies the installation everywhere else; anything the installer may call a folder therefore
/// has to survive being offered.
/// </remarks>
/// <param name="appSettingsStore">Settings naming the folder the versions are installed in.</param>
/// <param name="directoryReader">Reads what the install folder contains.</param>
/// <param name="loggerFactory">Creates the log a failed read is reported to.</param>
public sealed partial class InstalledRuntimeVersionsProvider(
    IAppSettingsStore appSettingsStore,
    IDirectoryReader directoryReader,
    IAppLoggerFactory loggerFactory)
    : IInstalledRuntimeVersionsProvider
{
    private readonly IAppLogger logger = loggerFactory.CreateLogger(LogSources.App);

    /// <inheritdoc/>
    public InstalledRuntimeVersions Read()
    {
        var installFolderPath = appSettingsStore.Current.IdeInstallFolderPath;

        IReadOnlyList<string> folderNames;
        try
        {
            folderNames = directoryReader.ReadSubFolderNames(installFolderPath);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            this.logger.Warning(
                "The IDE install folder could not be read, so there are no runtime versions to " +
                $"choose from ({installFolderPath}).",
                exception.ToString());

            return new InstalledRuntimeVersions(Versions: [], IsInstallFolderReadable: false);
        }

        return new InstalledRuntimeVersions(Order(folderNames), IsInstallFolderReadable: true);
    }

    /// <summary>
    /// Puts the version folder names in the order they are offered in: newest first.
    /// </summary>
    /// <remarks>
    /// The order the console application established, kept so that the same install folder offers
    /// the same first choice in both. Names that are a number throughout are ordered as numbers;
    /// anything else sorts after them and therefore, once reversed, appears at the top — a name
    /// carrying a suffix is a version the tester singled out and is the one they are most likely
    /// looking for.
    /// </remarks>
    /// <param name="folderNames">Names of the folders found in the install folder.</param>
    /// <returns>The names that are versions, newest first.</returns>
    private static IReadOnlyList<string> Order(IEnumerable<string> folderNames)
        =>
        [
            ..folderNames
                .Where(static name => !string.IsNullOrEmpty(name) && LeadingNumber().IsMatch(name))
                .OrderBy(static name => int.TryParse(name, out var parsed) ? parsed : char.MaxValue)
                .ThenBy(static name =>
                    int.TryParse(LeadingNumber().Match(name).Value, out var parsed)
                        ? parsed
                        : char.MaxValue)
                .ThenBy(static name => name, StringComparer.Ordinal)
                .Reverse()
        ];

    [GeneratedRegex(@"^\d+")]
    private static partial Regex LeadingNumber();
}
