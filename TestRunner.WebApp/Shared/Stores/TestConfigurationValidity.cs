namespace TestRunner.WebApp.Shared.Stores;

using System.Linq;

/// <summary>
/// What checking a test configuration found: nothing, or what is wrong with it.
/// </summary>
/// <param name="Errors">Everything found wrong, in the order the fields are shown in.</param>
public sealed record TestConfigurationValidity(IReadOnlyList<TestConfigurationError> Errors)
{
    /// <summary>
    /// A configuration with nothing wrong with it.
    /// </summary>
    public static readonly TestConfigurationValidity Valid = new([]);

    /// <summary>
    /// Gets a value indicating whether the configuration may be used to run tests.
    /// </summary>
    public bool IsValid
        => this.Errors.Count == 0;

    /// <summary>
    /// Gets everything found wrong as one line, for telling the user why they cannot start.
    /// </summary>
    public string Summary
        => string.Join(" ", this.Errors.Select(static error => error.Message));

    /// <summary>
    /// Reads what was found wrong with one field.
    /// </summary>
    /// <param name="fieldName">Name of the field, see <see cref="TestConfigurationValues"/>.</param>
    /// <returns>The message, or <see langword="null"/> when the field is fine.</returns>
    public string? ErrorFor(string fieldName)
        => this.Errors
            .FirstOrDefault(error => string.Equals(error.FieldName, fieldName, StringComparison.Ordinal))
            ?.Message;
}

/// <summary>
/// One thing found wrong with a test configuration.
/// </summary>
/// <param name="FieldName">Field it was found on, see <see cref="TestConfigurationValues"/>.</param>
/// <param name="Message">What is wrong, as the tester reads it.</param>
public sealed record TestConfigurationError(string FieldName, string Message);
