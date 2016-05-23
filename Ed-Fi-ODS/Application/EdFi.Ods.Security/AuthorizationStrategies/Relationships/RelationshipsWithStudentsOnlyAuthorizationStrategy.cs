// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Linq;
using EdFi.Common.Security;
using EdFi.Common.Security.Authorization;
using EdFi.Ods.Common.Specifications;
using EdFi.Ods.Security.Authorization;

namespace EdFi.Ods.Security.AuthorizationStrategies.Relationships
{
    public class RelationshipsWithStudentsOnlyAuthorizationStrategy<TContextData> :
        RelationshipsAuthorizationStrategyBase<TContextData>
        where TContextData : RelationshipsAuthorizationContextData, new()
    {
        public RelationshipsWithStudentsOnlyAuthorizationStrategy(
            IConcreteEducationOrganizationIdAuthorizationContextDataTransformer<TContextData>
                concreteEducationOrganizationIdAuthorizationContextDataTransformer,
            IRelationshipsAuthorizationContextDataProviderFactory<TContextData>
                relationshipsAuthorizationContextDataProviderFactory,
            IAuthorizationSegmentsToFiltersConverter authorizationSegmentsToFiltersConverter,
            IEducationOrganizationCache educationOrganizationCache,
            IEducationOrganizationHierarchyProvider educationOrganizationHierarchyProvider,
            IAuthorizationSegmentsVerifier authorizationSegmentsVerifier)
            : base(
                concreteEducationOrganizationIdAuthorizationContextDataTransformer,
                relationshipsAuthorizationContextDataProviderFactory,
                authorizationSegmentsToFiltersConverter,
                educationOrganizationCache,
                educationOrganizationHierarchyProvider,
                authorizationSegmentsVerifier)
        {
        }

        protected override void BuildAuthorizationSegments(
            AuthorizationBuilder<TContextData> authorizationBuilder,
            Type entityType,
            string[] authorizationContextPropertyNames)
        {
            authorizationBuilder.ClaimsMustBeAssociatedWith(
                authorizationContextPropertyNames
                    .Where(p => PersonEntitySpecification.IsPersonIdentifier(p, "Student"))
                    .ToArray());
        }
    }
}