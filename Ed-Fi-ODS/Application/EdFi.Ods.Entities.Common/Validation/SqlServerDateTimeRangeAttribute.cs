using System;
using System.ComponentModel.DataAnnotations;

namespace EdFi.Ods.Entities.Common.Validation
{
    /// <summary>
    /// Ensures that the value assigned falls within SQL server's datetime range
    /// </summary>
    public class SqlServerDateTimeRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;
            if (!(value is DateTime))
                throw new ArgumentException("SqlServerDateTimeRangeAttribute can only be used with DateTime types");

            var dt = (DateTime) value;
            
            if (dt >= System.Data.SqlTypes.SqlDateTime.MinValue.Value &&
                dt <= System.Data.SqlTypes.SqlDateTime.MaxValue.Value)
            {
                return ValidationResult.Success;
            }

            var contextualDisplayName = validationContext == null ? string.Empty : validationContext.DisplayName;
            return
                new ValidationResult(
                    string.Format(
                        "{0} : '{1}' must be within SQL datetime range ('{2}' to '{3}')",
                        contextualDisplayName, value, System.Data.SqlTypes.SqlDateTime.MinValue.Value,
                        System.Data.SqlTypes.SqlDateTime.MaxValue.Value));
        }
    }
}