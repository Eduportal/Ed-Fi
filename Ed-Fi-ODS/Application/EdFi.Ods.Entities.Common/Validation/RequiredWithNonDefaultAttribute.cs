using System.ComponentModel.DataAnnotations;
using EdFi.Common;
using EdFi.Common.Extensions;
using EdFi.Ods.Common;

namespace EdFi.Ods.Entities.Common.Validation
{
    /// <summary>
    /// Ensures that the value assigned is not the Type's default value (e.g. "null" for reference types, 0's for numbers, etc.).
    /// </summary>
    public class RequiredWithNonDefaultAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string s = value as string;

            if (s != null)
            {
                if (!string.IsNullOrEmpty(s))
                    return ValidationResult.Success;
            }
            else if (value is bool)
            {
                // A default boolean value (false) is a reasonable value
                return ValidationResult.Success;
            }
            else if (value is decimal)
            {
                // In case of decimal types, accept default values
                return ValidationResult.Success;
            }
            else if (value != null && !value.Equals(value.GetType().GetDefaultValue()))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(string.Format("{0} is required.", validationContext.DisplayName));
        }
    }
}
