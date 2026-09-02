namespace TestRunner.WebApp.Shared.Stores;

/// <summary>
/// The application settings, as the features that do not edit them read them.
/// </summary>
/// <remarks>
/// Reading only. Changing the settings is what the AppSettings feature is for, so it owns the store
/// that writes them and nobody else is handed a way to.
/// </remarks>
public interface IAppSettingsStore
{
    /// <summary>
    /// Raised after the settings have changed.
    /// </summary>
    event Action? Changed;

    /// <summary>
    /// Gets the settings as they stand now.
    /// </summary>
    AppSettingsState Current { get; }
}
