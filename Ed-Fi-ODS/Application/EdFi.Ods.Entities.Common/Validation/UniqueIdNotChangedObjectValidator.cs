using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Specifications;
using EdFi.Ods.Entities.Common.Caching;

namespace EdFi.Ods.Entities.Common.Validation
{
    public class UniqueIdNotChangedObjectValidator : ObjectValidatorBase, IObjectValidator
    {
        private readonly IPersonUniqueIdToIdCache _personIdentifiersCache;

        public UniqueIdNotChangedObjectValidator(IPersonUniqueIdToIdCache personIdentifiersCache)
        {
            _personIdentifiersCache = personIdentifiersCache;
        }

        public virtual ICollection<ValidationResult> ValidateObject(object @object)
        {
            var validationResults = new List<ValidationResult>();
            var objType = @object.GetType();
            if (PersonEntitySpecification.IsPersonEntity(objType))
            {
                var objectWithIdentifier = (IHasIdentifier)@object;

                var persistedUniqueId = _personIdentifiersCache.GetUniqueId(objType.Name, objectWithIdentifier.Id);
                
                var objectWithUniqueId = (IIdentifiablePerson)@object;
                string newUniqueId = objectWithUniqueId.UniqueId;

                if (persistedUniqueId != null && persistedUniqueId != newUniqueId)
                    validationResults.Add(new ValidationResult("A person's UniqueId cannot be modified."));
            }

            if (validationResults.Any())
                SetInvalid();
            else
                SetValid();

            return validationResults;
        }
    }
}
