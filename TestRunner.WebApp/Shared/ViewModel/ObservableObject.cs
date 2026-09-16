namespace TestRunner.WebApp.Shared.ViewModel;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using TestRunner.WebApp.Shared.Validation;

public class ViewModelBase
    : CommunityToolkit.Mvvm.ComponentModel.ObservableObject,
      INotifyValidityInfo
{
    private readonly ConcurrentDictionary<string, Validity> propertyValidities = new();

    /// <inheritdoc/>
    public event Action<bool>? HasErrorsChanged;

    /// <inheritdoc/>
    public bool HasErrors
        => this.propertyValidities.All(x => x.Value.HasErrors);

    /// <inheritdoc/>
    public Validity? GetValidity(string propertyName)
        => this.propertyValidities.TryGetValue(propertyName, out var validity)
            ? validity
            : null;

    /// <summary>
    /// Validates the specified property using the provided validator function.
    /// </summary>
    /// <typeparam name="TValue">The type of the property being validated.</typeparam>
    /// <param name="value">The value of the property being validated.</param>
    /// <param name="validator">The function used to validate the property.</param>
    /// <param name="propertyName">The name of the property to validate.</param>
    /// <returns>The validity result of the property.</returns>
    protected Validity ValidateProperty<TValue>(
        TValue value,
        Func<TValue, Validity> validator,
        [CallerMemberName] string propertyName = "")
    {
        var validity = validator(value);

        if (this.propertyValidities.TryGetValue(propertyName, out var looked) && looked == validity)
        {
            return validity;
        }

        this.propertyValidities[propertyName] = validity;

        var oldHasErrors = this.HasErrors;
        this.propertyValidities[propertyName] = validity;
        var newHasErrors = this.HasErrors;

        if (oldHasErrors != newHasErrors)
        {
            this.HasErrorsChanged?.Invoke(newHasErrors);
        }

        // What is wrong with one value is often another value's doing, so a control showing any of
        // them is told that there is something new to read rather than only the one just looked at.
        this.OnPropertyChanged(nameof(this.HasErrors));

        return validity;
    }
}