using EdFi.Ods.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EdFi.Ods.Entities.Common.Validation
{
    public class DataAnnotationsObjectValidator : ObjectValidatorBase, IObjectValidator
    {
        public virtual ICollection<ValidationResult> ValidateObject(object @object)
        {
            var validationResults = new List<ValidationResult>();

            Validator.TryValidateObject(@object, new ValidationContext(@object, null, null), validationResults, true);

            if (validationResults.Any())
                SetInvalid();
            else
                SetValid();

            return validationResults;
        }
    }
}