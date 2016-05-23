using System.Collections.Generic;
using System.ComponentModel;
using EdFi.Ods.Security.Metadata.Contexts;
using EdFi.Ods.Security.Metadata.Models;
using EdFi.Ods.Security.Metadata.Repositories;


namespace EdFi.Ods.WebService.Tests.Owin
{
    public class OwinSecurityRepository : SecurityRepositoryBase, ISecurityRepository
    {
        public OwinSecurityRepository()
        {
            var data = new SeedData();
            Intitalize(data.OdsApplication, data.Actions, data.ClaimSets, data.ResourceClaims, data.AuthorizationStrategies, data.ClaimSetResourceClaims, data.ResourceClaimAuthorizationStrategies);
        }

        public Application GetApplication()
        {
            return Application;
        }

        public List<Action> GetActions()
        {
            return Actions;
        }

        public List<ClaimSet> GetClaimSets()
        {
            return ClaimSets;
        }
        
        public List<ResourceClaim> GetResourceClaims()
        {
            return ResourceClaims;
        }

        public List<AuthorizationStrategy> GetAuthorizationStrategies()
        {
            return AuthorizationStrategies;
        }

        public List<ClaimSetResourceClaim> GetClaimSetResourceClaims()
        {
            return ClaimSetResourceClaims;
        }

        public List<ResourceClaimAuthorizationStrategy> GetResourceClaimAuthorizationStrategies()
        {
            return ResourceClaimAuthorizationStrategies;
        }

        public void ReIntitalize(Application application,
            List<Action> actions,
            List<ClaimSet> claimSets,
            List<ResourceClaim> resourceClaims,
            List<AuthorizationStrategy> authorizationStrategies,
            List<ClaimSetResourceClaim> claimSetResourceClaims,
            List<ResourceClaimAuthorizationStrategy> resourceClaimAuthorizationStrategies)
        {
            Intitalize(application, actions, claimSets, resourceClaims, authorizationStrategies, claimSetResourceClaims, resourceClaimAuthorizationStrategies);
        }
        
    }
}
