using System;
using System.Collections.Generic;
using EdFi.Ods.Security.Metadata.Models;
using EdFi.Ods.Security.Metadata.Repositories;
using Action = EdFi.Ods.Security.Metadata.Models.Action;

namespace EdFi.Ods.Tests.EdFi.Ods.Security._Stubs
{
    public class StubSecurityRepository : ISecurityRepository
    {
        public Action GetActionByName(string actionName)
        {
            return new Action { ActionId = 1, ActionName = actionName, ActionUri = "http://ed-fi.org/ods/actions/" + actionName.ToLowerInvariant() };
        }

        public AuthorizationStrategy GetAuthorizationStrategyByName(string authorizationStrategyName)
        {
            return new AuthorizationStrategy { Application = new Application { ApplicationId = 1 }, AuthorizationStrategyId = 1, AuthorizationStrategyName = authorizationStrategyName, DisplayName = authorizationStrategyName };
        }

        public IEnumerable<ClaimSetResourceClaim> GetClaimsForClaimSet(string claimSetName)
        {
            return new List<ClaimSetResourceClaim>();
        }

        public IEnumerable<string> GetClaimsForResource(string resourceUri)
        {
            return new List<string>();
        }

        public IEnumerable<ResourceClaimAuthorizationStrategy> GetClaimAndStrategyForResource(string resourceUri, string action)
        {
            return new List<ResourceClaimAuthorizationStrategy>();
        }

        public Action GetActionByHttpVerb(string httpVerb)
        {
            var actionName = string.Empty;
            switch (httpVerb)
            {
                case "GET":
                    actionName = "Read";
                    break;
                case "POST":
                    actionName = "Upsert";
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

        public ResourceClaim GetResourceByResourceName(string resourceName)
        {
            return new ResourceClaim { ResourceName = resourceName };
        }
    }
}