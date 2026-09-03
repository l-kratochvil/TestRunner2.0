namespace TestRunner.WebApp.Shared.Stores;

using System.Text.Json.Serialization;

/// <summary>
/// The application settings describing the installation rather than one test run.
/// </summary>
/// <remarks>
/// Shared by every browser because
/// <see cref="TestRunner.WebApp.Features.AppSettings.Services.AppSettingsStore"/> keeps them in a
/// file on the server.
/// </remarks>
/// <param name="IdeInstallFolderPath">
/// IDE install folder. Each subdirectory under it is treated as one installed runtime version.
/// </param>
public record AppSettingsState(
    [property: JsonPropertyName("ideInstallFolderPath")]
    string IdeInstallFolderPath);
