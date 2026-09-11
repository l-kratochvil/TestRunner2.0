namespace TestRunner.WebApp.Shared.Validation;

using System.Linq;

/// <summary>
/// The result of validating a test configuration.
/// </summary>
/// <param name="Errors">Problems found, in the order the fields are shown in.</param>
public sealed record TestConfigurationValidity(IReadOnlyList<TestConfigurationError> Errors)
{
    /// <summary>
    /// A valid test configuration.
    /// </summary>
    public static readonly TestConfigurationValidity Valid = new([]);

    /// <summary>
    /// Gets a value indicating whether the configuration may be used for a test run.
    /// </summary>
    public bool IsValid
        => this.Errors.Count == 0;

    /// <summary>
    /// Gets all problem messages as one line for the tester.
    /// </summary>
    public string Summary
        => string.Join(" ", this.Errors.Select(static error => error.Message));

    /// <summary>
    /// Reads the problem message for one field.
    /// </summary>
    /// <param name="fieldName">Name of a property on <see cref="TestConfigurationValues"/>.</param>
    /// <returns>The message for <paramref name="fieldName"/>, or <see langword="null"/>.</returns>
    public string? ErrorFor(string fieldName)
        => this.Errors
            .FirstOrDefault(error => string.Equals(error.FieldName, fieldName, StringComparison.Ordinal))
            ?.Message;
}

/// <summary>
/// One problem found in a test configuration.
/// </summary>
/// <param name="FieldName">Name of a property on <see cref="TestConfigurationValues"/>.</param>
/// <param name="Message">Problem text shown to the tester.</param>
public sealed record TestConfigurationError(string FieldName, string Message);
