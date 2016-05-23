using System.Collections.Generic;

namespace EdFi.Common.Security.Claims
{
    /// <summary>
    /// Defines a method for obtaining a list of claims (and the corresponding authorization strategies) 
    /// that can be use to authorize a resourceUri.
    /// </summary>
    public interface IResourceAuthorizationMetadataProvider
    {
        IEnumerable<ResourceClaimAuthorizationStrategy> GetResourceClaimAuthorizationStrategies(string resourceUri, string action);
    }

    public class ResourceClaimAuthorizationStrategy
    {
        /// <summary>
        /// The URI of the claim type.
        /// </summary>
        public string ClaimName { get; set; }

        /// <summary>
        /// The name of the strategy to be used in the authorization decision.
        /// </summary>
        public string AuthorizationStrategy { get; set; }
    }
}
