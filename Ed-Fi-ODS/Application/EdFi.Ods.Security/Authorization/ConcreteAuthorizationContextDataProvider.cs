using System;
using EdFi.Common.Security;
using EdFi.Ods.Common.Exceptions;
using EdFi.Ods.Security.AuthorizationStrategies.Relationships;

namespace EdFi.Ods.Security.Authorization
{
    /// <summary>
    /// Transforms the provided <see cref="RelationshipsAuthorizationContextData"/> instance such that the abstract 
    /// <see cref="RelationshipsAuthorizationContextData.EducationOrganizationId"/> property is replaced with the
    /// known concrete identifer type (e.g. LocalEducationAgencyId, SchoolId, etc.).
    /// </summary>
    public class ConcreteEducationOrganizationIdAuthorizationContextDataTransformer<TContextData>
        : IConcreteEducationOrganizationIdAuthorizationContextDataTransformer<TContextData>
        where TContextData : RelationshipsAuthorizationContextData, new()
    {
        private readonly IEducationOrganizationCache _educationOrganizationCache;

        public ConcreteEducationOrganizationIdAuthorizationContextDataTransformer(IEducationOrganizationCache educationOrganizationCache)
        {
            _educationOrganizationCache = educationOrganizationCache;
        }

        public virtual TContextData GetConcreteAuthorizationContextData(TContextData authorizationContextData)
        {
            var educationOrganizationId = authorizationContextData.EducationOrganizationId;

            if (educationOrganizationId == null)
                return authorizationContextData;

            var identifiers = _educationOrganizationCache.GetEducationOrganizationIdentifiers(
                educationOrganizationId.GetValueOrDefault());

            // If no identifier Tuple could be found, we can't authorize this request
            if (identifiers == null)
                throw new NotFoundException(string.Format(
                    "Education Organization with an identifier of '{0}' could not be found.",
                    educationOrganizationId));

            TContextData concreteAuthorizationContextData = (TContextData) authorizationContextData.Clone();

            // TODO: For EdOrg extensibility, this will need some attention.
            switch (identifiers.EducationOrganizationType)
            {
                case "StateEducationAgency":
                    concreteAuthorizationContextData.StateEducationAgencyId = educationOrganizationId;
                    concreteAuthorizationContextData.ConcreteEducationOrganizationIdPropertyName = "StateEducationAgencyId";
                    break;
                case "EducationServiceCenter":
                    concreteAuthorizationContextData.EducationServiceCenterId = educationOrganizationId;
                    concreteAuthorizationContextData.ConcreteEducationOrganizationIdPropertyName = "EducationServiceCenterId";
                    break;
                case "LocalEducationAgency":
                    concreteAuthorizationContextData.LocalEducationAgencyId = educationOrganizationId;
                    concreteAuthorizationContextData.ConcreteEducationOrganizationIdPropertyName = "LocalEducationAgencyId";
                    break;
                case "School":
                    concreteAuthorizationContextData.SchoolId = educationOrganizationId;
                    concreteAuthorizationContextData.ConcreteEducationOrganizationIdPropertyName = "SchoolId";
                    break;
                case "EducationOrganizationNetwork":
                    concreteAuthorizationContextData.EducationOrganizationNetworkId = educationOrganizationId;
                    concreteAuthorizationContextData.ConcreteEducationOrganizationIdPropertyName = "EducationOrganizationNetworkId";
                    break;
                default:
                    throw new NotSupportedException(string.Format("Unhandled EducationOrganizationType of '{0}' encountered while translating an abstract EducationOrganizationId to a more concrete identifier type during authorization.",
                        identifiers.EducationOrganizationType));
            }

            // Clear the abstract identifier
            concreteAuthorizationContextData.EducationOrganizationId = null;

            return concreteAuthorizationContextData;
        }
    }
}
