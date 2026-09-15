namespace TestRunner.WebApp.Shared.Validation;

using System.Linq.Expressions;

public class Validator<TValidated>(TValidated validated)
{
    public Validity Validate<TValue>(
        Expression<Func<TValidated, TValue>> binder,
        TValue value)
    {
        var memberExpression = (MemberExpression)binder.Body;
        var propertyName = memberExpression.Member.Name;

        // Here you would implement the actual validation logic.
        // For demonstration purposes, let's assume the property is always valid.
        return Validity.Valid;
    }
}