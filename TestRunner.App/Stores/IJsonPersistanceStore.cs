namespace TestRunner.App.Stores;

internal interface IJsonPersistanceStore<TJsonModel>
{
    TJsonModel Current { get; }

    void Update(Func<TJsonModel, TJsonModel> updator);

    void Update(TJsonModel currentModel);
}