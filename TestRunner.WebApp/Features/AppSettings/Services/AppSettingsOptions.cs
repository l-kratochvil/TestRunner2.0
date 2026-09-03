namespace TestRunner.WebApp.Features.AppSettings.Services;

/// <summary>
/// Options configuring where the application settings file is kept.
/// </summary>
/// <remarks>
/// The application settings describe the installation. The file path configures the store and is
/// not part of those settings.
/// </remarks>
public sealed class AppSettingsOptions
{
    /// <summary>
    /// Gets the full path of the application settings file.
    /// </summary>
    public string FilePath { get; init; } = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TestRunner.WebApp",
        "user-settings.json");
}
