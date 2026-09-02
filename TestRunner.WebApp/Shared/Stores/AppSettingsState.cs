namespace TestRunner.WebApp.Shared.Stores;

using System.Text.Json.Serialization;

/// <summary>
/// The settings of the installation: what the machine the application runs on looks like, as
/// opposed to what one tester is about to run.
/// </summary>
/// <remarks>
/// Kept in a file on the server and therefore the same for everyone connecting, see
/// <c>AppSettingsStore</c>.
/// </remarks>
/// <param name="IdeInstallFolderPath">
/// Folder the IDE is installed in. Every runtime version installed under it is a folder of its own,
/// which is where the runtime versions offered to the tester come from.
/// </param>
public record AppSettingsState(
    [property: JsonPropertyName("ideInstallFolderPath")]
    string IdeInstallFolderPath);
