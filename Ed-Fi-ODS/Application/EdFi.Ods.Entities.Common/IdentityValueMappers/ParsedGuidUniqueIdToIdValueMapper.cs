using System;

namespace EdFi.Ods.Entities.Common.IdentityValueMappers
{
    public class ParsedGuidUniqueIdToIdValueMapper : IUniqueIdToIdValueMapper
    {
        public PersonIdentifiersValueMap GetId(string personType, string uniqueId)
        {
            return new PersonIdentifiersValueMap
            {
                Id = Guid.Parse(uniqueId),
                UniqueId = uniqueId
            };
        }

        public PersonIdentifiersValueMap GetUniqueId(string personType, Guid id)
        {
            return new PersonIdentifiersValueMap
            {
                Id = id,
                UniqueId = id.ToString("N")
            };
        }
    }
}
