namespace TestRunner.WebApp.Features.AppSettings.Services;

/// <summary>
/// Configuration of where the application settings are kept, bound from the <c>AppSettings</c>
/// configuration section.
/// </summary>
/// <remarks>
/// The settings the tester edits describe the installation; where the file holding them lives is
/// not one of them, which is why it is configuration of the store rather than part of its state.
/// </remarks>
public sealed class AppSettingsOptions
{
    /// <summary>
    /// Gets the full path of the file the settings are kept in.
    /// </summary>
    public string FilePath { get; init; } = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TestRunner.WebApp",
        "user-settings.json");
}
