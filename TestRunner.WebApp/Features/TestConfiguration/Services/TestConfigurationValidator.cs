namespace TestRunner.WebApp.Features.TestConfiguration.Services;

using System.Text.RegularExpressions;

using FluentValidation;

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
    private readonly IValidator<ITestConfigurationValidationSource> rules = new Rules();

    /// <inheritdoc/>
    public Validity Validate(ITestConfigurationValidationSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return this.rules.Validate(source).ToValidity();
    }

    // Semantic versioning with the patch left out, which is how the IDE versions are written down.
    [GeneratedRegex(@"^\d+\.\d+(\.\d+)?$")]
    private static partial Regex IdeVersionFormat();

    /// <remarks>
    /// Nested and private, so that FluentValidation stays an implementation detail of the rules
    /// rather than something every caller has to know about. The failures it produces are keyed by
    /// the property names of <see cref="ITestConfigurationValidationSource"/>, which is what the
    /// configurator looks its messages up by.
    /// </remarks>
    private sealed class Rules : AbstractValidator<ITestConfigurationValidationSource>
    {
        public Rules()
        {
            this.RuleFor(static source => source.RuntimeVersion)
                .NotEmpty()
                .WithMessage("Select the runtime version the tests run against.");

            // A runtime test runs against hardware, so which station it runs on is part of what is
            // tested rather than a detail of the run.
            this.When(
                static source => source.IsRuntimeTestSelected,
                () => this.RuleFor(static source => source.TestedHwAssembly)
                    .Must(static station => station is not null and not TestedHwAssemblyType.Unknown)
                    .WithMessage("Select the test station the runtime tests run on."));

            // The version is what the result is filed under in TestLink, so it is only ever needed
            // when the result goes there.
            this.When(
                static source => source.IsTestLinkEnabled,
                () => this.RuleFor(static source => source.IdeVersionText)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage("Enter the IDE version the result is filed under.")
                    .Matches(IdeVersionFormat())
                    .WithMessage("Write the IDE version as x.y or x.y.z, for example 6.1 or 6.1.4."));
        }
    }
}
