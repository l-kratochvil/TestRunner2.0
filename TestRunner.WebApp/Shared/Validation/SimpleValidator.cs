    namespace TestRunner.WebApp.Shared.Validation;

    using FluentValidation;

    public class SimpleValidator<TValidated>
    {
        private readonly InlineValidator<TValidated> innerValidator;

        public SimpleValidator(
            Func<IRuleBuilderInitial<TValidated, TValidated>, IRuleBuilderOptions<TValidated, TValidated>> ruleBuilder)
        {
            this.innerValidator = [];
            ruleBuilder(this.innerValidator.RuleFor(x => x));
        }

        public Validity Validate(TValidated validated)
            => this.innerValidator.Validate(validated).ToValidity();
    }