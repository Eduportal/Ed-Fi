using System;
using System.Linq;
using EdFi.Ods.Common.Specifications;
using EdFi.Ods.Entities.Common.IdentityValueMappers;

namespace EdFi.Ods.Entities.Common.UniqueIdIntegration
{
    /// <summary>
    ///     Implements identity value mappers that assume that the <b>UniqueIdIntegrationContext</b> database and the
    ///     <b>EdFi_ODS</b> currently in context for the request are deployed to the same SQL Server instance
    ///     and the ODS person tables are accessible to the credentials provided for <b>UniqueIdIntegrationContext</b>.
    /// </summary>
    public class UniqueIdIntegrationBasedIdValueMapper : IUniqueIdToIdValueMapper
    {
        public PersonIdentifiersValueMap GetId(string personType, string uniqueId)
        {
            return Get(personType, uniqueId);
        }

        public PersonIdentifiersValueMap GetUniqueId(string personType, Guid id)
        {
            return Get(personType, id);
        }

        private PersonIdentifiersValueMap Get(string personTypeName, string uniqueId)
        {
            var results = new PersonIdentifiersValueMap {UniqueId = uniqueId};

            ValidatePersonType(personTypeName);
            
            using (var db = new UniqueIdIntegrationContext())
            {
                var uniqueIdMapping = db.UniqueIdPersonMappings.FirstOrDefault(mapping => mapping.UniqueId == uniqueId);
                
                if (uniqueIdMapping == null)
                    return results;

                results.Id = uniqueIdMapping.Id;
            }

            return results;
        }

        private PersonIdentifiersValueMap Get(string personTypeName, Guid id)
        {
            var results = new PersonIdentifiersValueMap {Id = id};

            ValidatePersonType(personTypeName);

            using (var db = new UniqueIdIntegrationContext())
            {
                var uniqueIdMapping = db.UniqueIdPersonMappings.FirstOrDefault(mapping => mapping.Id == id);

                if (uniqueIdMapping == null)
                    return results;

                results.UniqueId = uniqueIdMapping.UniqueId;
            }

            return results;
        }

        private static void ValidatePersonType(string personTypeName)
        {
            if (!PersonEntitySpecification.IsPersonEntity(personTypeName))
                throw new ArgumentException(
                    string.Format("Invalid person type '{0}'. Valid person types are: {1}", personTypeName,
                                  "'" + String.Join("','", PersonEntitySpecification.ValidPersonTypes) + "'"));
        }
    }
}