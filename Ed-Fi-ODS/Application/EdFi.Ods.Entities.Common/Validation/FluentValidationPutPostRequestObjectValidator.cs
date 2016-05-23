// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EdFi.Common.Extensions;
using EdFi.Ods.Common;
using FluentValidation;

namespace EdFi.Ods.Entities.Common.Validation
{
    public class FluentValidationPutPostRequestObjectValidator : ObjectValidatorBase, IObjectValidator
    {
        private static readonly ConcurrentDictionary<Type, IValidator> ValidatorByType
            = new ConcurrentDictionary<Type, IValidator>();

        public ICollection<ValidationResult> ValidateObject(object @object)
        {
            Type objectType = @object.GetType();

            // Only handle PUT and POST request objects
            // TODO: Embedded convention - request class names, and namespace
            if (!objectType.Name.EndsWith("Put")
                && !objectType.Name.EndsWith("Post")
                || !objectType.FullName.Contains(".Requests."))
            {
                // No validation to be performed
                return new List<ValidationResult>();
            }

            var validator = ValidatorByType.GetOrAdd(
                objectType,
                t =>
                {
                    // Locate the validator by convention
                    string typeName;

                    if (!objectType.Name.TryTrimSuffix("Put", out typeName)
                        && !objectType.Name.TryTrimSuffix("Post", out typeName))
                    {
                        throw new NotSupportedException(
                            string.Format(
                                "Only Put and Post request objects are supported by the '{0}'.",
                                typeof(FluentValidationPutPostRequestObjectValidator).Name));
                    }

                    // Assumption: Request classes are assumed to be derived directly from resource classes, 
                    // and the validator is generated with that class.
                    string validatorTypeName = string.Format("{0}, {1}",
                        objectType.BaseType.FullName + "PutPostRequestValidator",
                        objectType.BaseType.Assembly.GetName().FullName);

                    var validatorType = Type.GetType(validatorTypeName, throwOnError: false);

                    if (validatorType == null)
                        return null;

                    return (IValidator) Activator.CreateInstance(validatorType);
                });

            if (validator == null)
                return new List<ValidationResult>();

            var result = validator.Validate(@object);

            var validationResults = result.Errors.Select(e => 
                new ValidationResult(e.ErrorMessage)).ToList();
            
            return validationResults;
        }
    }
}