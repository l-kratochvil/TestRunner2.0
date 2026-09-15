namespace TestRunner.WebApp.Shared.ViewModel;

using System.Collections.Concurrent;
using TestRunner.WebApp.Shared.Validation;

public class ObservableObject
    : CommunityToolkit.Mvvm.ComponentModel.ObservableObject,
      INotifyValidityInfo
{
    public ConcurrentDictionary<string, Validity> propertyValidities = [];

    /// <inheritdoc/>
    public bool IsValid
        => this.propertyValidities.All(x => x.Value.IsValid);

    /// <inheritdoc/>
    public Validity? GetValidity(string propertyName)
        => this.propertyValidities.TryGetValue(propertyName, out var validity)
            ? validity
            : null;
}