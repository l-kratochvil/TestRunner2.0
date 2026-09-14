namespace TestRunner.WebApp.Shared.Validation;

/// <summary>
/// What the application settings are validated from.
/// </summary>
/// <remarks>
/// Implemented by whoever holds the settings while they are being edited, so that they are checked
/// where they are typed rather than after being copied somewhere else, and the field names the
/// problems are keyed by are the ones the editor already knows.
/// </remarks>
public interface IAppSettingsValidationSource
{
    /// <summary>
    /// Gets the IDE install folder path as it stands.
    /// </summary>
    string IdeInstallFolderPath { get; }
}