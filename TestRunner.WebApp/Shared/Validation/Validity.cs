namespace TestRunner.WebApp.Shared.Validation;

using System.Linq;

/// <summary>
/// The problems found in something, in the order the fields are shown in.
/// </summary>
/// <remarks>
/// Every validator in the application answers in this, so a second feature does not invent a third
/// way of saying that a field is wrong.
/// </remarks>
/// <param name="Problems">Problems found.</param>
public sealed record Validity(IReadOnlyList<Validity.Problem> Problems)
{
    /// <summary>
    /// Nothing wrong.
    /// </summary>
    public static readonly Validity Valid = new([]);

    /// <summary>
    /// How serious a <see cref="Problem"/> is.
    /// </summary>
    public enum Severity
    {
        /// <summary>
        /// The value cannot be used.
        /// </summary>
        Error,

        /// <summary>
        /// The value can be used, but something about it is worth knowing.
        /// </summary>
        Warning,
    }

    /// <summary>
    /// Gets a value indicating whether what was validated may be used.
    /// </summary>
    /// <remarks>
    /// A warning explains something without blocking it, so only an error makes this false.
    /// </remarks>
    public bool IsValid
        => !this.Problems.Any(static problem => problem.Severity is Severity.Error);

    /// <summary>
    /// Gets all problem messages as one line, for wherever a single summary is shown.
    /// </summary>
    public string Summary
        => string.Join(" ", this.Problems.Select(static problem => problem.Message));

    /// <summary>
    /// Narrows this down to one field.
    /// </summary>
    /// <param name="fieldName">Field to ask about.</param>
    /// <returns>The <see cref="Validity"/> of <paramref name="fieldName"/> alone.</returns>
    public Validity For(string fieldName)
        => new(
            [
                ..this.Problems.Where(
                    problem => string.Equals(problem.FieldName, fieldName, StringComparison.Ordinal))
            ]);

    /// <summary>
    /// One problem found in what was validated.
    /// </summary>
    /// <param name="FieldName">Name of the field the problem belongs to.</param>
    /// <param name="Message">Problem text shown to the tester.</param>
    /// <param name="Severity">How serious the problem is.</param>
    public sealed record Problem(
        string FieldName,
        string Message,
        Severity Severity = Severity.Error);
}
