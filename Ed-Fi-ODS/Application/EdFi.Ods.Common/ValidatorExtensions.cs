using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace EdFi.Ods.Common
{
    public static class ObjectValidatorExtensions
    {
        public static ICollection<ValidationResult> ValidateObject(this IEnumerable<IObjectValidator> validators, object @object)
        {
            var result = new List<ValidationResult>();
            if (validators != null)
            {
                foreach (var validator in validators)
                {
                    try
                    {
                        result.AddRange(validator.ValidateObject(@object));
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Validation exception [{0}]: {1}", ex.GetType(), ex.StackTrace);
                        result.Add(new ValidationResult(ex.Message));
                    }
                }
            }

            return result;
        }
    }
}
