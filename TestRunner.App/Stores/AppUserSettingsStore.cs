namespace TestRunner.App.Stores;

using TestRunner.App;
using TestRunner.App.Model;

internal class AppUserSettingsStore : IJsonPersistanceStore<AppUserSettings>
{
    private const string DefaultIdeInstallationDirPath = @"C:\Program Files (x86)\Pertinax6";

    private readonly JsonPersistanceStore<AppUserSettings> jsonPersistanceStore = new(CreateDefaultAppUserSettings);

    private AppUserSettingsStore()
    {
    }

    /// <inheritdoc/>
    public AppUserSettings Current
        => this.jsonPersistanceStore.Current;

    public static AppUserSettingsStore Create()
        => JsonPersistanceStore<AppUserSettings>.InitStore(
            Paths.Files.AppSettings,
            CreateDefaultAppUserSettings,
            _ => new AppUserSettingsStore());

    /// <inheritdoc/>
    public void Update(Func<AppUserSettings, AppUserSettings> updator)
        => this.jsonPersistanceStore.Update(updator);

    /// <inheritdoc/>
    public void Update(AppUserSettings currentModel)
        => this.jsonPersistanceStore.Update(currentModel);

    private static AppUserSettings CreateDefaultAppUserSettings()
        => new(DefaultIdeInstallationDirPath);
}