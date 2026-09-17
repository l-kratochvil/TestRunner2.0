namespace TestRunner.WebApp.Shared.ViewModel;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using DevKit.Core.Extensions.Functional;

using FluentValidation;

using TestRunner.WebApp.Shared.Validation;

/// <summary>
/// Base class for view models, adding property validation on top of <see cref="CommunityToolkit.Mvvm.ComponentModel.ObservableObject"/>.
/// </summary>
public class ViewModelBase
    : CommunityToolkit.Mvvm.ComponentModel.ObservableObject,
      INotifyValidityInfo
{
    private readonly ConcurrentDictionary<string, Validity> propertyValidities = new();

    private IValidator? validator;

    /// <inheritdoc/>
    public event Action<bool>? HasErrorsChanged;

    /// <inheritdoc/>
    public bool HasErrors
        => this.propertyValidities.All(x => x.Value.HasErrors);

    /// <inheritdoc/>
    public Validity? GetValidity(string propertyName)
        => this.propertyValidities.GetValueOrDefault(propertyName);

    /// <summary>
    /// Initializes the validator for the view model.
    /// </summary>
    /// <typeparam name="TValidated">The type of the view model being validated.</typeparam>
    /// <param name="validated">The instance of the view model being validated.</param>
    /// <param name="initialized">An action to initialize the inline validator.</param>
    /// <exception cref="ArgumentException">Thrown if the validated instance is not of the same type as the view model.</exception>
    protected void InitValidator<TValidated>(
        TValidated validated, Action<InlineValidator<TValidated>> initialized)
        where TValidated : notnull
    {
        if (validated.GetType() != this.GetType())
        {
            throw new ArgumentException(
                "The validated instance must be of the same type as the view model.",
                nameof(validated));
        }

        var inlineValidator = new InlineValidator<TValidated>();

        initialized(inlineValidator);

        this.validator = inlineValidator;
    }

    /// <summary>
    /// Sets the property value and validates it.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="field">A reference to the backing field of the property.</param>
    /// <param name="newValue">The new value to assign.</param>
    /// <param name="propertyName">The name of the property being set.</param>
    /// <returns><see langword="true"/> if the value changed and validation passed; otherwise, <see langword="false"/>.</returns>
    protected new bool SetProperty<T>(
        [NotNullIfNotNull(nameof(newValue))] ref T field,
        T newValue,
        [CallerMemberName] string? propertyName = null)
        => !this.ValidateProperty(propertyName).HasErrors &&
           base.SetProperty(ref field, newValue, propertyName);

    /// <summary>
    /// Sets the property value and validates it.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="field">A reference to the backing field of the property.</param>
    /// <param name="newValue">The new value to assign.</param>
    /// <param name="comparer">The comparer used to determine whether the value changed.</param>
    /// <param name="propertyName">The name of the property being set.</param>
    /// <returns><see langword="true"/> if the value changed and validation passed; otherwise, <see langword="false"/>.</returns>
    protected new bool SetProperty<T>(
        [NotNullIfNotNull(nameof(newValue))] ref T field,
        T newValue,
        IEqualityComparer<T> comparer,
        [CallerMemberName] string? propertyName = null)
        => !this.ValidateProperty(propertyName).HasErrors &&
           base.SetProperty(ref field, newValue, comparer, propertyName);

    /// <summary>
    /// Sets the property value and validates it.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="oldValue">The current value of the property.</param>
    /// <param name="newValue">The new value to assign.</param>
    /// <param name="callback">The callback invoked with <paramref name="newValue"/> to store it.</param>
    /// <param name="propertyName">The name of the property being set.</param>
    /// <returns><see langword="true"/> if the value changed and validation passed; otherwise, <see langword="false"/>.</returns>
    protected new bool SetProperty<T>(
        T oldValue,
        T newValue,
        Action<T> callback,
        [CallerMemberName] string? propertyName = null)
        => !this.ValidateProperty(propertyName).HasErrors &&
           base.SetProperty(oldValue, newValue, callback, propertyName);

    /// <summary>
    /// Sets the property value and validates it.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="oldValue">The current value of the property.</param>
    /// <param name="newValue">The new value to assign.</param>
    /// <param name="comparer">The comparer used to determine whether the value changed.</param>
    /// <param name="callback">The callback invoked with <paramref name="newValue"/> to store it.</param>
    /// <param name="propertyName">The name of the property being set.</param>
    /// <returns><see langword="true"/> if the value changed and validation passed; otherwise, <see langword="false"/>.</returns>
    protected new bool SetProperty<T>(
        T oldValue,
        T newValue,
        IEqualityComparer<T> comparer,
        Action<T> callback,
        [CallerMemberName] string? propertyName = null)
        => !this.ValidateProperty(propertyName).HasErrors &&
           base.SetProperty(oldValue, newValue, comparer, callback, propertyName);

    /// <summary>
    /// Sets the property value and validates it.
    /// </summary>
    /// <typeparam name="TModel">The type of the model owning the backing field.</typeparam>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="oldValue">The current value of the property.</param>
    /// <param name="newValue">The new value to assign.</param>
    /// <param name="model">The model instance passed to <paramref name="callback"/>.</param>
    /// <param name="callback">The callback invoked with <paramref name="model"/> and <paramref name="newValue"/> to store it.</param>
    /// <param name="propertyName">The name of the property being set.</param>
    /// <returns><see langword="true"/> if the value changed and validation passed; otherwise, <see langword="false"/>.</returns>
    protected new bool SetProperty<TModel, T>(
        T oldValue,
        T newValue,
        TModel model,
        Action<TModel, T> callback,
        [CallerMemberName] string? propertyName = null)
        where TModel : class
        => !this.ValidateProperty(propertyName).HasErrors &&
           base.SetProperty(oldValue, newValue, model, callback, propertyName);

    /// <summary>
    /// Sets the property value and validates it.
    /// </summary>
    /// <typeparam name="TModel">The type of the model owning the backing field.</typeparam>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="oldValue">The current value of the property.</param>
    /// <param name="newValue">The new value to assign.</param>
    /// <param name="comparer">The comparer used to determine whether the value changed.</param>
    /// <param name="model">The model instance passed to <paramref name="callback"/>.</param>
    /// <param name="callback">The callback invoked with <paramref name="model"/> and <paramref name="newValue"/> to store it.</param>
    /// <param name="propertyName">The name of the property being set.</param>
    /// <returns><see langword="true"/> if the value changed and validation passed; otherwise, <see langword="false"/>.</returns>
    protected new bool SetProperty<TModel, T>(
        T oldValue,
        T newValue,
        IEqualityComparer<T> comparer,
        TModel model,
        Action<TModel, T> callback,
        [CallerMemberName] string? propertyName = null)
        where TModel : class
        => !this.ValidateProperty(propertyName).HasErrors &&
           base.SetProperty(oldValue, newValue, comparer, model, callback, propertyName);

    /// <summary>
    /// Validates the specified property using the view model's validator.
    /// </summary>
    /// <param name="propertyName">The name of the property to validate.</param>
    /// <returns>The validity result of the property.</returns>
    protected Validity ValidateProperty(string? propertyName)
    {
        if (this.validator is null || propertyName is null)
        {
            return Validity.Valid;
        }

        var validity = ValidationContext<object>
            .CreateWithOptions(this, x => x.IncludeProperties(propertyName))
            .Pipe(this.validator.Validate)
            .ToValidity();

        if (this.propertyValidities.TryGetValue(propertyName, out var looked) && looked == validity)
        {
            return validity;
        }

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