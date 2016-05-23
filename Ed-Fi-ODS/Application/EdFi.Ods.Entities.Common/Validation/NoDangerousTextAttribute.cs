using System;
using System.ComponentModel.DataAnnotations;

namespace EdFi.Ods.Entities.Common.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NoDangerousTextAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string s = value as string;

            if (string.IsNullOrEmpty(s))
                return ValidationResult.Success;

            if (CrossSiteScriptingValidation.IsCrossSiteScriptDanger(s))
                return new ValidationResult(string.Format("{0} contains a potentially dangerous value.", validationContext.DisplayName));

            return ValidationResult.Success;
        }
    }
}
