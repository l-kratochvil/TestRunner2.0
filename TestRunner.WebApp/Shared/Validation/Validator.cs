namespace TestRunner.WebApp.Shared.Validation;

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

public abstract class Validator<TValidated>
{
    private readonly ConcurrentDictionary<string, Func<TValidated, Validity>> propertyValidators = new();

    private readonly TValidated validated;

    protected Validator(TValidated validated)
    {
        this.validated = validated;
    }

    /// <summary>
    /// Registers a validator for the specified property.
    /// </summary>
    /// <typeparam name="TValue">The type of the property being validated.</typeparam>
    /// <param name="propertyAccessor">An expression that accesses the property to validate.</param>
    /// <param name="validator">A function that validates the property value and returns a Validity result.</param>
    /// <returns>The current validator instance for method chaining.</returns>
    public Validator<TValidated> ValidatorFor<TValue>(
        Expression<Func<TValidated, TValue>> propertyAccessor,
        Func<TValue, Validity> validator)
    {
        var property = ReadBoundProperty(propertyAccessor);

        // The rule is kept as one that reads the property itself, so the value never leaves the
        // type it was written against and is never cast back out of an object. The accessor is
        // compiled here rather than at every validation, because a rule is added once.
        var readValue = propertyAccessor.Compile();

        this.propertyValidators[property.Name] = value => validator(readValue(value));

        return this;
    }

    /// <summary>
    /// Validates the specified property of the validated object.
    /// </summary>
    /// <typeparam name="TValue">The type of the property being validated.</typeparam>
    /// <param name="propertyAccessor">An expression that accesses the property to validate.</param>
    /// <returns>
    /// The <see cref="Validity"/> the rule added for the property answers, or
    /// <see cref="Validity.Valid"/> where no rule was added for it.
    /// </returns>
    public Validity Validate<TValue>(
        Expression<Func<TValidated, TValue>> propertyAccessor)
    {
        var property = ReadBoundProperty(propertyAccessor);

        // A property nobody wrote a rule for is nothing to complain about, which is not the same as
        // a property whose rule found nothing wrong, but reads the same to whoever is shown it.
        return this.propertyValidators.TryGetValue(property.Name, out var validator)
            ? validator(this.validated)
            : Validity.Valid;
    }

    private static PropertyInfo ReadBoundProperty<TValue>(
        Expression<Func<TValidated, TValue>> propertyAccessor)
    {
        if (propertyAccessor.Body is not MemberExpression
            {
                Member: PropertyInfo property,
                Expression: ParameterExpression,
            })
        {
            throw new InvalidOperationException(
                $"The binding of a {typeof(Validator<TValidated>).Name} must name a property of " +
                $"{typeof(TValidated).Name} directly.");
        }

        return property;
    }
}