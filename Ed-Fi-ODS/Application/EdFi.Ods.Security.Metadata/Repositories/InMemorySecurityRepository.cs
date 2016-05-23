using EdFi.Ods.Security.Metadata.Models;
using System.Collections.Generic;
using Action = EdFi.Ods.Security.Metadata.Models.Action;
using Application = EdFi.Ods.Security.Metadata.Models.Application;

namespace EdFi.Ods.Security.Metadata.Repositories
{
    public class InMemorySecurityRepository : SecurityRepositoryBase, ISecurityRepository
    {
        public InMemorySecurityRepository()
        {
            var application = new Application { ApplicationId = 1, ApplicationName = "Console" };
            var claimSets = new List<ClaimSet> { new ClaimSet { Application = application, ClaimSetId = 1, ClaimSetName = "Console" }};
            var actions = InitializeActions();
            var resourceClaims = InitializeResourceClaims(application);
            var authorizationStrategies = InitializeAuthorizationStrategies(application);
            var claimSetResourceClaims = InitializeClaimSetResourceClaims(resourceClaims, actions, claimSets[0]);
            var resourceClaimAuthorizationStrategies = InitializeResourceClaimAuthorizationStrategies(actions, authorizationStrategies, resourceClaims);

            Intitalize(application, actions, claimSets, resourceClaims, authorizationStrategies, claimSetResourceClaims, resourceClaimAuthorizationStrategies);

        }

        private List<Action> InitializeActions()
        {
            return new List<Action>
            {
                new Action { ActionId = 1, ActionName = "Create", ActionUri = "http://ed-fi.org/ods/actions/create" },
                new Action { ActionId = 2, ActionName = "Read", ActionUri = "http://ed-fi.org/ods/actions/read" },
                new Action { ActionId = 3, ActionName = "Update", ActionUri = "http://ed-fi.org/ods/actions/update" },
                new Action { ActionId = 4, ActionName = "Delete", ActionUri = "http://ed-fi.org/ods/actions/delete" },
                new Action { ActionId = 5, ActionName = "Upsert", ActionUri = "http://ed-fi.org/ods/actions/upsert" },
            };
        }
        private List<ClaimSetResourceClaim> InitializeClaimSetResourceClaims(List<ResourceClaim> resourceClaims, List<Action> actions, ClaimSet claimSet)
        {
            return new List<ClaimSetResourceClaim>
            {
                new ClaimSetResourceClaim { ClaimSetResourceClaimId = 1,  ResourceClaim = resourceClaims[0], Action = actions[1], ClaimSet = claimSet },
                new ClaimSetResourceClaim { ClaimSetResourceClaimId = 2,  ResourceClaim = resourceClaims[1], Action = actions[1], ClaimSet = claimSet },
                new ClaimSetResourceClaim { ClaimSetResourceClaimId = 3,  ResourceClaim = resourceClaims[2], Action = actions[1], ClaimSet = claimSet },
                
                new ClaimSetResourceClaim { ClaimSetResourceClaimId = 4,  ResourceClaim = resourceClaims[3], Action = actions[0], ClaimSet = claimSet },
                new ClaimSetResourceClaim { ClaimSetResourceClaimId = 5,  ResourceClaim = resourceClaims[3], Action = actions[1], ClaimSet = claimSet },
                new ClaimSetResourceClaim { ClaimSetResourceClaimId = 6,  ResourceClaim = resourceClaims[3], Action = actions[2], ClaimSet = claimSet },
                new ClaimSetResourceClaim { ClaimSetResourceClaimId = 7,  ResourceClaim = resourceClaims[3], Action = actions[3], ClaimSet = claimSet },
                new ClaimSetResourceClaim { ClaimSetResourceClaimId = 8,  ResourceClaim = resourceClaims[3], Action = actions[4], ClaimSet = claimSet },
                
                new ClaimSetResourceClaim { ClaimSetResourceClaimId = 11,  ResourceClaim = resourceClaims[4], Action = actions[0], ClaimSet = claimSet },
                new ClaimSetResourceClaim { ClaimSetResourceClaimId = 12,  ResourceClaim = resourceClaims[4], Action = actions[1], ClaimSet = claimSet },
                new ClaimSetResourceClaim { ClaimSetResourceClaimId = 13,  ResourceClaim = resourceClaims[4], Action = actions[2], ClaimSet = claimSet },
                new ClaimSetResourceClaim { ClaimSetResourceClaimId = 14,  ResourceClaim = resourceClaims[4], Action = actions[3], ClaimSet = claimSet },
                new ClaimSetResourceClaim { ClaimSetResourceClaimId = 15,  ResourceClaim = resourceClaims[4], Action = actions[4], ClaimSet = claimSet },
            };
        }
        private List<AuthorizationStrategy> InitializeAuthorizationStrategies(Application application)
        {
            return new List<AuthorizationStrategy>
            {
                new AuthorizationStrategy { AuthorizationStrategyId = 1, DisplayName = "AllRelationships", AuthorizationStrategyName = "AllRelationships", Application = application },
                new AuthorizationStrategy { AuthorizationStrategyId = 2, DisplayName = "EdFiDescriptors", AuthorizationStrategyName = "EdFiDescriptors", Application = application },
                new AuthorizationStrategy { AuthorizationStrategyId = 3, DisplayName = "EdFiTypes", AuthorizationStrategyName = "EdFiTypes", Application = application },
                new AuthorizationStrategy { AuthorizationStrategyId = 4, DisplayName = "EducationOrganizations", AuthorizationStrategyName = "EducationOrganizations", Application = application },
                new AuthorizationStrategy { AuthorizationStrategyId = 5, DisplayName = "NoFurtherAuthorizationRequired", AuthorizationStrategyName = "NoFurtherAuthorizationRequired", Application = application },
                new AuthorizationStrategy { AuthorizationStrategyId = 6, DisplayName = "PrimaryRelationships", AuthorizationStrategyName = "PrimaryRelationships", Application = application },
            };
        }
        private List<ResourceClaim> InitializeResourceClaims(Application application)
        {
            var generalData = new ResourceClaim { ResourceClaimId = 5, ClaimName = "http://ed-fi.org/ods/identity/claims/domains/generalData", DisplayName = "generalData", ResourceName = "generalData", ParentResourceClaimId = null, ParentResourceClaim = null, Application = application };

            return new List<ResourceClaim>
            {
                new ResourceClaim{ResourceClaimId = 1,  ClaimName = "http://ed-fi.org/ods/identity/claims/domains/edFiTypes", DisplayName = "types", ResourceName = "types", ParentResourceClaimId = null, ParentResourceClaim = null, Application = application},
                new ResourceClaim{ResourceClaimId = 2,  ClaimName = "http://ed-fi.org/ods/identity/claims/domains/edFiDescriptors", DisplayName = "descriptors", ResourceName = "descriptors", ParentResourceClaimId = null, ParentResourceClaim = null, Application = application},
                new ResourceClaim{ResourceClaimId = 3,  ClaimName = "http://ed-fi.org/ods/identity/claims/domains/educationOrganizations", DisplayName = "educationOrganizations", ResourceName = "educationOrganizations", ParentResourceClaimId = null, ParentResourceClaim = null, Application = application},
                new ResourceClaim{ResourceClaimId = 4,  ClaimName = "http://ed-fi.org/ods/identity/claims/domains/people", DisplayName = "people", ResourceName = "people", ParentResourceClaimId = null, ParentResourceClaim = null, Application = application},
                generalData,
                new ResourceClaim{ResourceClaimId = 6,  ClaimName = "http://ed-fi.org/ods/identity/claims/course", DisplayName = "course", ResourceName = "course", ParentResourceClaimId = 5, ParentResourceClaim = generalData, Application = application},
                new ResourceClaim{ResourceClaimId = 7,  ClaimName = "http://ed-fi.org/ods/identity/claims/staffEducationOrganizationAssignmentAssociation", DisplayName = "staffEducationOrganizationAssignmentAssociation", ResourceName = "staffEducationOrganizationAssignmentAssociation", ParentResourceClaimId = 5, ParentResourceClaim = generalData, Application = application},
                new ResourceClaim{ResourceClaimId = 8,  ClaimName = "http://ed-fi.org/ods/identity/claims/staffEducationOrganizationEmploymentAssociation", DisplayName = "staffEducationOrganizationEmploymentAssociation", ResourceName = "staffEducationOrganizationEmploymentAssociation", ParentResourceClaimId = 5, ParentResourceClaim = generalData, Application = application},
                new ResourceClaim{ResourceClaimId = 9,  ClaimName = "http://ed-fi.org/ods/identity/claims/studentParentAssociation", DisplayName = "studentParentAssociation", ResourceName = "studentParentAssociation", ParentResourceClaimId = 5, ParentResourceClaim = generalData, Application = application},
                new ResourceClaim{ResourceClaimId = 10, ClaimName = "http://ed-fi.org/ods/identity/claims/studentSchoolAssociation", DisplayName = "studentSchoolAssociation", ResourceName = "studentSchoolAssociation", ParentResourceClaimId = 5, ParentResourceClaim = generalData, Application = application},
            };
        }
        private List<ResourceClaimAuthorizationStrategy> InitializeResourceClaimAuthorizationStrategies(List<Action> actions, List<AuthorizationStrategy> authorizationStrategies, List<ResourceClaim> resourceClaims)
        {
            return new List<ResourceClaimAuthorizationStrategy>
            {
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[1], AuthorizationStrategy = authorizationStrategies[2], ResourceClaim = resourceClaims[0], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[1], AuthorizationStrategy = authorizationStrategies[1], ResourceClaim = resourceClaims[1], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[1], AuthorizationStrategy = authorizationStrategies[3], ResourceClaim = resourceClaims[2], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[1], AuthorizationStrategy = authorizationStrategies[4], ResourceClaim = resourceClaims[5], Scheme = null },
                
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[0], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[6], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[1], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[6], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[2], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[6], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[3], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[6], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[4], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[6], Scheme = null },
                
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[0], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[7], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[1], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[7], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[2], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[7], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[3], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[7], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[4], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[7], Scheme = null },
                
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[0], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[8], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[1], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[8], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[2], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[8], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[3], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[8], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[4], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[8], Scheme = null },
                
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[0], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[9], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[1], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[9], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[2], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[9], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[3], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[9], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[4], AuthorizationStrategy = authorizationStrategies[5], ResourceClaim = resourceClaims[9], Scheme = null },
                
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[0], AuthorizationStrategy = authorizationStrategies[4], ResourceClaim = resourceClaims[3], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[1], AuthorizationStrategy = authorizationStrategies[0], ResourceClaim = resourceClaims[3], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[2], AuthorizationStrategy = authorizationStrategies[0], ResourceClaim = resourceClaims[3], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[3], AuthorizationStrategy = authorizationStrategies[4], ResourceClaim = resourceClaims[3], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[4], AuthorizationStrategy = authorizationStrategies[0], ResourceClaim = resourceClaims[3], Scheme = null },
                
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[0], AuthorizationStrategy = authorizationStrategies[0], ResourceClaim = resourceClaims[4], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[1], AuthorizationStrategy = authorizationStrategies[0], ResourceClaim = resourceClaims[4], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[2], AuthorizationStrategy = authorizationStrategies[0], ResourceClaim = resourceClaims[4], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[3], AuthorizationStrategy = authorizationStrategies[0], ResourceClaim = resourceClaims[4], Scheme = null },
                new ResourceClaimAuthorizationStrategy { ResourceClaimAuthorizationStrategyId = 1, Action = actions[4], AuthorizationStrategy = authorizationStrategies[0], ResourceClaim = resourceClaims[4], Scheme = null },
            };
        }
    }
}
