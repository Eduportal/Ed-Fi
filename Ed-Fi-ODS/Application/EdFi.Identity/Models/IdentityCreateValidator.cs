namespace EdFi.Identity.Models
{
    using System;

    using EdFi.Common.Identity;

    using FluentValidation;

    public class IdentityResourceCreateValidator : AbstractValidator<IdentityResource>
    {
        public IdentityResourceCreateValidator()
        {

            this.RuleFor(x => x.GivenNames)
                .NotEmpty();

            this.RuleFor(x => x.FamilyNames)
                .NotEmpty();

            var validValuesForGender = string.Join(" ,", typeof (Gender).GetEnumNames());

            this.RuleFor(x => x.BirthGender)
                   .MustBeValidEnumValue<IdentityResource, Gender>()
                   .WithMessage("'{0}' is not a valid value for Birth Gender. Valid values are: {1}", x => x.BirthGender, x => validValuesForGender)
                   .When(x => !string.IsNullOrWhiteSpace(x.BirthGender));


            this.RuleFor(x => x.BirthDate)
                .MustBePastDate()
                .WithMessage("Birth Date cannot be in the future.")
                .When(x => x.BirthDate.HasValue);
        }

        private static Func<DateTime?, bool> BeAPastDate
        {
            get { return x => x.Value <= DateTime.Now; }
        }

        private static Func<string, bool> BeValidGenderValue()
        {
            return birthGender =>
            {
                Gender parsedValue;
                return Enum.TryParse(birthGender, true, out parsedValue);
            };
        }
    }

    internal static class RuleBuilderExtensions
    {
        public static IRuleBuilderOptions<T, DateTime?> MustBePastDate<T>(this IRuleBuilder<T, DateTime?> ruleBuilder)
        {
            return ruleBuilder.Must(x => x.Value <= DateTime.Now);
        }

        public static IRuleBuilderOptions<T, string> MustBeValidEnumValue<T, TEnum>(this IRuleBuilder<T, string> ruleBuilder) where TEnum : struct
        {
            return ruleBuilder.Must(x =>
            {
                TEnum parsedValue;
                return Enum.TryParse(x, true, out parsedValue);
            });
        }
    }
}