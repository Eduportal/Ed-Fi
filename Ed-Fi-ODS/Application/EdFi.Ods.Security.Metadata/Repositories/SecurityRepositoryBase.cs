using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdFi.Ods.Security.Metadata.Models;
using Action = EdFi.Ods.Security.Metadata.Models.Action;

namespace EdFi.Ods.Security.Metadata.Repositories
{
    public abstract class SecurityRepositoryBase
    {
        protected Application Application { get; private set; }
        protected List<Action> Actions { get; private set; }
        protected List<ClaimSet> ClaimSets { get; private set; }
        protected List<ResourceClaim> ResourceClaims { get; private set; }
        protected List<AuthorizationStrategy> AuthorizationStrategies { get; private set; }
        protected List<ClaimSetResourceClaim> ClaimSetResourceClaims { get; private set; }
        protected List<ResourceClaimAuthorizationStrategy> ResourceClaimAuthorizationStrategies { get; private set; }

        protected void Intitalize(Application application, 
                                    List<Action> actions, 
                                    List<ClaimSet> claimSets, 
                                    List<ResourceClaim> resourceClaims,
                                    List<AuthorizationStrategy> authorizationStrategies, 
                                    List<ClaimSetResourceClaim> claimSetResourceClaims, 
                                    List<ResourceClaimAuthorizationStrategy> resourceClaimAuthorizationStrategies)
        {
            Application = application;
            Actions = actions;
            ClaimSets = claimSets;
            ResourceClaims = resourceClaims;
            AuthorizationStrategies = authorizationStrategies;
            ClaimSetResourceClaims = claimSetResourceClaims;
            ResourceClaimAuthorizationStrategies = resourceClaimAuthorizationStrategies;
        }

        public Action GetActionByHttpVerb(string httpVerb)
        {
            string actionName = string.Empty;
            switch (httpVerb)
            {
                case "GET":
                    actionName = "Read";
                    break;
                case "POST":
                    actionName = "Create";
                    break;
                case "PUT":
                    actionName = "Update";
                    break;
                case "DELETE":
                    actionName = "Delete";
                    break;
            }

            return GetActionByName(actionName);
        }

        public Action GetActionByName(string actionName)
        {
            return Actions.First(a => a.ActionName.Equals(actionName, StringComparison.InvariantCultureIgnoreCase));
        }

        public AuthorizationStrategy GetAuthorizationStrategyByName(string authorizationStrategyName)
        {
            return AuthorizationStrategies.First(a => a.AuthorizationStrategyName.Equals(authorizationStrategyName, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<ClaimSetResourceClaim> GetClaimsForClaimSet(string claimSetName)
        {
            return ClaimSetResourceClaims.Where(c => c.ClaimSet.ClaimSetName.Equals(claimSetName, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<string> GetClaimsForResource(string resourceUri)
        {
            return GetResourceClaims(resourceUri).Select(c => c.ClaimName);
        }
        private IEnumerable<ResourceClaim> GetResourceClaims(string resourceUri)
        {
            var claims = new List<ResourceClaim>();

            var resourceSpecificClaim = ResourceClaims.Where(rc => rc.ResourceName.Equals(resourceUri, StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (resourceSpecificClaim.Any())
            {
                claims.AddRange(resourceSpecificClaim);

                foreach (var resourceClaim in resourceSpecificClaim)
                {
                    if (resourceClaim.ParentResourceClaim != null)
                    {
                        claims.AddRange(GetResourceClaims(resourceClaim.ParentResourceClaim.ResourceName));
                    }
                }
            }

            return claims;
        }

        // TODO: GKM 3/19 - Check this merge conflict resolution.
        public IEnumerable<ResourceClaimAuthorizationStrategy> GetClaimAndStrategyForResource(string resourceUri, string action)
        {
            var strategies = new List<ResourceClaimAuthorizationStrategy>();
			
            AddStrategiesForResourceClaimLineage(strategies, resourceUri, action);

            return strategies;
        }

        private void AddStrategiesForResourceClaimLineage(List<ResourceClaimAuthorizationStrategy> strategies, string resourceUri, string action)
        {
            var resourceName = ResourceClaim.GetResourceName(resourceUri);

            //check for exact match on resource and action
            var claimAndStrategy = ResourceClaimAuthorizationStrategies
                .SingleOrDefault(rcas => 
                    rcas.ResourceClaim.ResourceName.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase)
                    && rcas.Action.ActionUri.Equals(action, StringComparison.InvariantCultureIgnoreCase));

            // Add the claim/strategy if it was found
            if (claimAndStrategy != null)
                strategies.Add(claimAndStrategy);

            var resourceClaim = ResourceClaims.FirstOrDefault(rc => rc.ResourceName.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));

            // if there's a parent resource, recurse
            if ((resourceClaim != null) && (resourceClaim.ParentResourceClaim != null))
                AddStrategiesForResourceClaimLineage(strategies, resourceClaim.ParentResourceClaim.ResourceName, action);
        }
		
        public ResourceClaim GetResourceByResourceName(string resourceName)
        {
            return ResourceClaims.FirstOrDefault(rc => rc.ResourceName.Equals(resourceName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
