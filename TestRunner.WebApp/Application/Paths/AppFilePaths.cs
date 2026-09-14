namespace TestRunner.WebApp.Application.Paths;

/// <summary>
/// The files of this application.
/// </summary>
/// <param name="UserSettings">Full path of the file the application settings are kept in.</param>
public sealed record AppFilePaths(
    string UserSettings);
