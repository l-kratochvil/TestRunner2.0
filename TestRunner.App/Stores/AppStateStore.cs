namespace TestRunner.App.Stores;

using TestRunner.App.Model;

internal class AppStateStore : IJsonPersistanceStore<AppState>
{
    private readonly JsonPersistanceStore<AppState> jsonPersistanceStore = new(CreateDefaultAppState);

    private AppStateStore()
    {
    }

    /// <inheritdoc/>
    public AppState Current
        => this.jsonPersistanceStore.Current;

    public static AppStateStore Create()
        => JsonPersistanceStore<AppState>.InitStore(
            Paths.Files.AppSettings,
            CreateDefaultAppState,
            _ => new AppStateStore());

    private static AppState CreateDefaultAppState()
        => new(RuntimeVersion: null, IdeVersion: null);

    /// <inheritdoc/>
    public void Update(Func<AppState, AppState> updator)
        => this.jsonPersistanceStore.Update(updator);

    /// <inheritdoc/>
    public void Update(AppState currentModel)
        => this.jsonPersistanceStore.Update(currentModel);
}