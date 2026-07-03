namespace TestRunner.App.Stores;

using System;
using System.Text.Json;

internal class JsonPersistanceStore<TJsonModel>(
    Func<TJsonModel> defaultModelFactory)
    : IJsonPersistanceStore<TJsonModel>
{
    /// <inheritdoc/>
    public TJsonModel Current { get; private set; } = defaultModelFactory();

    public static TStore InitStore<TStore>(
        string jsonModelPath,
        Func<TJsonModel> defaultModelFactory,
        Func<TJsonModel, TStore> storeFactory)
    {
        TJsonModel jsonModel;
        if (File.Exists(jsonModelPath))
        {
            jsonModel = File.ReadAllText(jsonModelPath)
                            .Pipe(json => JsonSerializer.Deserialize<TJsonModel>(
                                json, JsonPersistanceStore.JsonSerializerOptions))
                        ?? defaultModelFactory();
        }
        else
        {
            jsonModel = defaultModelFactory();
            Save(jsonModel);
        }

        return storeFactory(jsonModel);
    }

    /// <inheritdoc/>
    public void Update(Func<TJsonModel, TJsonModel> updator)
        => this.Update(updator(this.Current));

    /// <inheritdoc/>
    public void Update(TJsonModel currentSettings)
    {
        this.Current = currentSettings;
        JsonSerializer
            .Serialize(this.Current, JsonPersistanceStore.JsonSerializerOptions)
            .Visit(serialized => File.WriteAllText(Paths.Files.AppSettings, serialized));
    }

    private static void Save(TJsonModel jsonModel)
        => JsonSerializer
            .Serialize(jsonModel, JsonPersistanceStore.JsonSerializerOptions)
            .Visit(serialized => File.WriteAllText(Paths.Files.AppSettings, serialized));
}

internal static class JsonPersistanceStore
{
    public static readonly JsonSerializerOptions JsonSerializerOptions =
        new()
        {
            WriteIndented = true,
        };
}