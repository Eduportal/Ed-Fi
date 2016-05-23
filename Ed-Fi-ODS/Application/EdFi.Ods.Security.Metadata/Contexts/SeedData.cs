using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.Ods.Security.Metadata.Models;
using Action = EdFi.Ods.Security.Metadata.Models.Action;

namespace EdFi.Ods.Security.Metadata.Contexts
{
    internal partial class SeedData
    {
        internal readonly List<Action> Actions;
        internal readonly List<AuthorizationStrategy> AuthorizationStrategies;
        internal readonly List<ClaimSet> ClaimSets;
        internal readonly List<ClaimSetResourceClaim> ClaimSetResourceClaims;
        internal readonly List<ResourceClaimAuthorizationStrategy> ResourceClaimAuthorizationStrategies;

        internal SeedData()
        {
            Actions = InitializeActions();
            AuthorizationStrategies = InitializeAuthorizationStrategies();
            ClaimSets = InitializeClaimSets();
            InitializeResourceClaims();
            ClaimSetResourceClaims = InitializeClaimSetResourceClaims();
            ResourceClaimAuthorizationStrategies = InitializeResourceClaimAuthorizationStrategies();
        }

        #region Actions
        internal readonly Action Create = new Action { ActionName = "Create", ActionUri = "http://ed-fi.org/odsapi/actions/create" };
        internal readonly Action Read = new Action { ActionName = "Read", ActionUri = "http://ed-fi.org/odsapi/actions/read" };
        internal readonly Action Update = new Action { ActionName = "Update", ActionUri = "http://ed-fi.org/odsapi/actions/update" };
        internal readonly Action Delete = new Action { ActionName = "Delete", ActionUri = "http://ed-fi.org/odsapi/actions/delete" };
        
        private List<Action> InitializeActions()
        {
            return new List<Action>
            {
                Create,
                Read,
                Update,
                Delete,
            };
        }
        #endregion

        #region Application
        internal readonly Application OdsApplication = new Application { ApplicationId = 1, ApplicationName = "Ed-Fi ODS API" };
        #endregion

        #region AuthorizationStrategies

        private AuthorizationStrategy NoFurtherAuthorizationStrategy            { get { return AuthorizationStrategies[0]; } }
        private AuthorizationStrategy RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy  { get { return AuthorizationStrategies[1]; } }
        private AuthorizationStrategy RelationshipsWithEdOrgsOnlyAuthorizationStrategy { get { return AuthorizationStrategies[2]; } }
        private AuthorizationStrategy NamespaceBasedAuthorizationStrategy        { get { return AuthorizationStrategies[3]; } }
        private AuthorizationStrategy AssessmentMetadataAuthorizationStrategy    { get { return AuthorizationStrategies[4]; } }
        private AuthorizationStrategy RelationshipsWithPeopleOnlyAuthorizationStrategy { get { return AuthorizationStrategies[5]; } }
        private AuthorizationStrategy RelationshipsWithStudentsOnlyAuthorizationStrategy { get { return AuthorizationStrategies[6]; } }

        internal List<AuthorizationStrategy> InitializeAuthorizationStrategies()
        {
            return new List<AuthorizationStrategy>
            {
                new AuthorizationStrategy { DisplayName = "No Further Authorization Required", AuthorizationStrategyName = "NoFurtherAuthorizationRequired", Application = OdsApplication },
                new AuthorizationStrategy { DisplayName = "Relationships with Education Organizations and People", AuthorizationStrategyName = "RelationshipsWithEdOrgsAndPeople", Application = OdsApplication },
                new AuthorizationStrategy { DisplayName = "Relationships with Education Organizations only", AuthorizationStrategyName = "RelationshipsWithEdOrgsOnly", Application = OdsApplication },
                new AuthorizationStrategy { DisplayName = "Namespace Based", AuthorizationStrategyName = "NamespaceBased", Application = OdsApplication },
                new AuthorizationStrategy { DisplayName = "Assessment Metadata", AuthorizationStrategyName = "AssessmentMetadata", Application = OdsApplication },
                new AuthorizationStrategy { DisplayName = "Relationships with People only", AuthorizationStrategyName = "RelationshipsWithPeopleOnly", Application = OdsApplication },
                new AuthorizationStrategy { DisplayName = "Relationships with Students only", AuthorizationStrategyName = "RelationshipsWithStudentsOnly", Application = OdsApplication },
            };
        }
        #endregion

        #region ClaimSets
        internal List<ClaimSet> InitializeClaimSets()
        {
            return new List<ClaimSet> { new ClaimSet { ClaimSetId = 1, ClaimSetName = "SIS Vendor", Application = OdsApplication }, };
        }
        #endregion

        partial void InitializeResourceClaims();

        #region ClaimSetResourceClaims
        internal List<ClaimSetResourceClaim> InitializeClaimSetResourceClaims()
        {
            return new List<ClaimSetResourceClaim>
            {
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("types")), Action = Read },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("systemDescriptors")), Action = Read },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationOrganizations")), Action = Read },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("people")), Action = Create },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("people")), Action = Read },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("people")), Action = Update },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("people")), Action = Delete },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("relationshipBasedData")), Action = Create },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("relationshipBasedData")), Action = Read },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("relationshipBasedData")), Action = Update },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("relationshipBasedData")), Action = Delete },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("assessmentMetadata")), Action = Create },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("assessmentMetadata")), Action = Read },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("assessmentMetadata")), Action = Update },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("assessmentMetadata")), Action = Delete },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("managedDescriptors")), Action = Create },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("managedDescriptors")), Action = Read },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("managedDescriptors")), Action = Update },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("managedDescriptors")), Action = Delete },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("identity")), Action = Create },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("identity")), Action = Read },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("identity")), Action = Update },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("assessmentFamily")), Action = Create },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("assessmentFamily")), Action = Read },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("assessmentFamily")), Action = Update },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("assessmentFamily")), Action = Delete },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("primaryRelationships")), Action = Create },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("primaryRelationships")), Action = Read },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("primaryRelationships")), Action = Update },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("primaryRelationships")), Action = Delete },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationStandards")), Action = Create },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationStandards")), Action = Read },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationStandards")), Action = Update },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationStandards")), Action = Delete },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationContent")), Action = Create },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationContent")), Action = Read },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationContent")), Action = Update },
                new ClaimSetResourceClaim { ClaimSet = ClaimSets[0], ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationContent")), Action = Delete },
            };
        }
        #endregion

        #region ResourceClaimAuthorizationStrategy
        internal List<ResourceClaimAuthorizationStrategy> InitializeResourceClaimAuthorizationStrategies()
        {
            return new List<ResourceClaimAuthorizationStrategy>
            {
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NoFurtherAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("types")), Action = Read},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NoFurtherAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("systemDescriptors")), Action = Read},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NoFurtherAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationOrganizations")), Action = Read},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NoFurtherAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("people")), Action = Create},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NoFurtherAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("people")), Action = Delete},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NoFurtherAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("course")), Action = Read},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = RelationshipsWithEdOrgsOnlyAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("primaryRelationships")), Action = Create},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("primaryRelationships")), Action = Read},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("primaryRelationships")), Action = Update},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("primaryRelationships")), Action = Delete},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = RelationshipsWithStudentsOnlyAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("studentParentAssociation")), Action = Create},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("studentParentAssociation")), Action = Read},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("studentParentAssociation")), Action = Update},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("studentParentAssociation")), Action = Delete},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("people")), Action = Read},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("people")), Action = Update},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("relationshipBasedData")), Action = Create},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("relationshipBasedData")), Action = Read},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("relationshipBasedData")), Action = Update},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = RelationshipsWithEdOrgsAndPeopleAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("relationshipBasedData")), Action = Delete},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NamespaceBasedAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("managedDescriptors")), Action = Create},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NoFurtherAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("managedDescriptors")), Action = Read},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NamespaceBasedAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("managedDescriptors")), Action = Update},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NamespaceBasedAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("managedDescriptors")), Action = Delete},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NoFurtherAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationOrganizations")), Action = Create},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NoFurtherAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationOrganizations")), Action = Update},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NoFurtherAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationOrganizations")), Action = Delete},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NamespaceBasedAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("assessmentFamily")), Action = Create},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NoFurtherAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("assessmentFamily")), Action = Read},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NamespaceBasedAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("assessmentFamily")), Action = Update},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NamespaceBasedAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("assessmentFamily")), Action = Delete},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = AssessmentMetadataAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("assessmentMetadata")), Action = Create},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = AssessmentMetadataAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("assessmentMetadata")), Action = Read},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = AssessmentMetadataAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("assessmentMetadata")), Action = Update},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = AssessmentMetadataAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("assessmentMetadata")), Action = Delete},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NoFurtherAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("identity")), Action = Create },
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NoFurtherAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("identity")), Action = Read },
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NoFurtherAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("identity")), Action = Update },
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NamespaceBasedAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationStandards")), Action = Create},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NamespaceBasedAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationStandards")), Action = Read},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NamespaceBasedAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationStandards")), Action = Update},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NamespaceBasedAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationStandards")), Action = Delete},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NamespaceBasedAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationContent")), Action = Create},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NamespaceBasedAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationContent")), Action = Read},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NamespaceBasedAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationContent")), Action = Update},
                new ResourceClaimAuthorizationStrategy {AuthorizationStrategy = NamespaceBasedAuthorizationStrategy, ResourceClaim = ResourceClaims.First(r => r.ResourceName.Equals("educationContent")), Action = Delete},
            };
        }
        #endregion
    }
}
