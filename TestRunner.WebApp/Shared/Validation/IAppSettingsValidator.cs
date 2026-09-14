namespace TestRunner.WebApp.Shared.Validation;

/// <summary>
/// Checks whether application settings can be saved.
/// </summary>
/// <remarks>
/// The AppSettings feature owns these rules so the editor is left with showing what they found, and
/// a second place cannot decide differently what an install folder has to be. It answers in
/// <see cref="Validity"/> like every other validator here.
/// </remarks>
public interface IAppSettingsValidator
{
    /// <summary>
    /// Checks <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Settings to check.</param>
    /// <returns>The problems found in <paramref name="source"/>.</returns>
    Validity Validate(IAppSettingsValidationSource source);
}
