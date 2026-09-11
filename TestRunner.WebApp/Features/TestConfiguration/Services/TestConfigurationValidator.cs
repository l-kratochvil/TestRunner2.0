namespace TestRunner.WebApp.Features.TestConfiguration.Services;

using System.Linq;
using System.Text.RegularExpressions;

using FluentValidation;
using FluentValidation.Results;

using TestRunner.WebApp.Shared.Domain;
using TestRunner.WebApp.Shared.Validation;

/// <summary>
/// The rules a test configuration has to meet before tests can be run with it.
/// </summary>
/// <remarks>
/// The rules are written with FluentValidation but kept behind
/// <see cref="ITestConfigurationValidator"/>, which answers in what the user interface shows. No
/// Blazor integration package takes part: the configurator draws its own rows and reads the
/// messages out of the result, so there is nothing left for an <c>EditForm</c> to add.
/// </remarks>
public sealed partial class TestConfigurationValidator : ITestConfigurationValidator
{
    private readonly IValidator<TestConfigurationValues> rules = new Rules();

    /// <inheritdoc/>
    public Validity Validate(TestConfigurationValues values)
    {
        ArgumentNullException.ThrowIfNull(values);

        ValidationResult result = this.rules.Validate(values);

        return result.IsValid
            ? Validity.Valid
            : new Validity(
                [
                    ..result.Errors.Select(
                        static failure => new Validity.Problem(
                            failure.PropertyName,
                            failure.ErrorMessage,
                            failure.Severity is FluentValidation.Severity.Error
                                ? Validity.Severity.Error
                                : Validity.Severity.Warning))
                ]);
    }

    // Semantic versioning with the patch left out, which is how the IDE versions are written down.
    [GeneratedRegex(@"^\d+\.\d+(\.\d+)?$")]
    private static partial Regex IdeVersionFormat();

    /// <remarks>
    /// Nested and private, so that FluentValidation stays an implementation detail of the rules
    /// rather than something every caller has to know about. The failures it produces are keyed by
    /// the property names of <see cref="TestConfigurationValues"/>, which is what the configurator
    /// looks its messages up by.
    /// </remarks>
    private sealed class Rules : AbstractValidator<TestConfigurationValues>
    {
        public Rules()
        {
            this.RuleFor(static values => values.RuntimeVersion)
                .NotEmpty()
                .WithMessage("Select the runtime version the tests run against.");

            // A runtime test runs against hardware, so which station it runs on is part of what is
            // tested rather than a detail of the run.
            this.When(
                static values => values.IsRuntimeTestSelected,
                () => this.RuleFor(static values => values.TestedHwAssembly)
                    .Must(static station => station is not null and not TestedHwAssemblyType.Unknown)
                    .WithMessage("Select the test station the runtime tests run on."));

            // The version is what the result is filed under in TestLink, so it is only ever needed
            // when the result goes there.
            this.When(
                static values => values.IsTestLinkEnabled,
                () => this.RuleFor(static values => values.IdeVersionText)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage("Enter the IDE version the result is filed under.")
                    .Matches(IdeVersionFormat())
                    .WithMessage("Write the IDE version as x.y or x.y.z, for example 6.1 or 6.1.4."));
        }
    }
}
