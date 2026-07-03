namespace TestRunner.App.Stores;

using TestRunner.App.Model;

internal class AppStateStore : IJsonPersistanceStore<AppState>
{
    private static readonly string JsonPath = Paths.Files.AppState;

    private readonly JsonPersistanceStore<AppState> jsonPersistanceStore = new(CreateDefaultAppState, JsonPath);

    private AppStateStore()
    {
    }

    /// <inheritdoc/>
    public AppState Current
        => this.jsonPersistanceStore.Current;

    public static AppStateStore Create()
        => JsonPersistanceStore<AppState>.InitStore(
            JsonPath,
            CreateDefaultAppState,
            model => new AppStateStore().Visit(x => x.Update(model)));

    private static AppState CreateDefaultAppState()
        => new(RuntimeVersion: null, IdeVersion: null);

    /// <inheritdoc/>
    public void Update(Func<AppState, AppState> updator)
        => this.jsonPersistanceStore.Update(updator);

    /// <inheritdoc/>
    public void Update(AppState currentModel)
        => this.jsonPersistanceStore.Update(currentModel);
}