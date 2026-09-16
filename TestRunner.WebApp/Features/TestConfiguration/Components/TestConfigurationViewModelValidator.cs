namespace TestRunner.WebApp.Features.TestConfiguration.Components;

using System.Text.RegularExpressions;
using FluentValidation;
using TestRunner.WebApp.Shared.Domain;
using TestRunner.WebApp.Shared.Validation;

internal partial class TestConfigurationViewModelValidator : Validator<TestConfigurationViewModel>
{
    public TestConfigurationViewModelValidator(
        TestConfigurationViewModel viewModel)
        : base(viewModel)
    {
        var runtimeVersionValidator = new SimpleValidator<string?>(
            rule => rule
            .NotEmpty()
            .WithMessage("Select the runtime version the tests run against."));

        this.ValidatorFor(
            x => x.RuntimeVersion,
            runtimeVersionValidator.Validate);

        var testedHwAssemblyValidator = new SimpleValidator<TestedHwAssemblyType?>(
            rule => rule
                .NotNull()
                .NotEqual(TestedHwAssemblyType.Unknown)
                .WithMessage("Select the test station the runtime tests run on."));

        this.ValidatorFor(
            x => x.TestedHwAssembly,
            testedHwAssemblyValidator.Validate);

        var ideVersionValidator = new SimpleValidator<Version?>(
            rule => rule
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("Enter the IDE version the result is filed under.")
                .Must(x => x is not null && IdeVersionFormat().IsMatch(x.ToString()))
                .WithMessage("Write the IDE version as x.y or x.y.z, for example 6.1 or 6.1.4."));

        this.ValidatorFor(
            x => x.IdeVersion,
            ideVersionValidator.Validate);
    }

    // Semantic versioning with the patch left out, which is how the IDE versions are written down.
    [GeneratedRegex(@"^\d+\.\d+(\.\d+)?$")]
    private static partial Regex IdeVersionFormat();
}