using System.Collections.Generic;
using System.Linq;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Security.Metadata.Repositories;

namespace EdFi.Ods.Security.AuthorizationStrategies
{
    public class ResourceAuthorizationMetadataProvider : IResourceAuthorizationMetadataProvider
    {
        private readonly ISecurityRepository _securityRepository;

        public ResourceAuthorizationMetadataProvider(
            ISecurityRepository securityRepository)
        {
            _securityRepository = securityRepository;
        }

        public IEnumerable<ResourceClaimAuthorizationStrategy> GetResourceClaimAuthorizationStrategies(string resourceUri, string action)
        {
            //hit the ResourceClaimAuthorizationStrategy table
            return
                _securityRepository
                    .GetClaimAndStrategyForResource(resourceUri, action)
                    .Select(
                        s => new ResourceClaimAuthorizationStrategy
                        {
                            ClaimName = s.ResourceClaim.ClaimName,
                            AuthorizationStrategy = s.AuthorizationStrategy.AuthorizationStrategyName
                        });
        }
    }
}
