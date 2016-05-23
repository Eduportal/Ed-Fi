using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EdFi.Ods.Common
{
    public interface IObjectValidator
    {
        ICollection<ValidationResult> ValidateObject(object @object);
    }
}