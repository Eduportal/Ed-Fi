using EdFi.Ods.Security.Metadata.Models;
using System.Collections.Generic;


namespace EdFi.Ods.Security.Metadata.Repositories
{
    public interface ISecurityRepository
    {
        Action GetActionByHttpVerb(string httpVerb);
        Action GetActionByName(string actionName);
        AuthorizationStrategy GetAuthorizationStrategyByName(string authorizationStrategyName);
        IEnumerable<ClaimSetResourceClaim> GetClaimsForClaimSet(string claimSetName);
        IEnumerable<string> GetClaimsForResource(string resourceUri);
        IEnumerable<ResourceClaimAuthorizationStrategy> GetClaimAndStrategyForResource(string resourceUri, string action);
        ResourceClaim GetResourceByResourceName(string resourceName);
    }
}
