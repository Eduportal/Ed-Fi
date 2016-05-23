using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using EdFi.Common.Security;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Common.Security;
using EdFi.Ods.Security.Metadata.Repositories;

namespace EdFi.Ods.Security.Claims
{
    public class ClaimsIdentityProvider : IClaimsIdentityProvider
    {
        private readonly IApiKeyContextProvider _apiKeyContextProvider;
        private readonly ISecurityRepository _securityRepository;

        public ClaimsIdentityProvider(IApiKeyContextProvider apiKeyContextProvider, ISecurityRepository securityRepository)
        {
            _apiKeyContextProvider = apiKeyContextProvider;
            _securityRepository = securityRepository;
        }

        public ClaimsIdentity GetClaimsIdentity()
        {
            // Get the Education Organization Ids for the current context
            var apiKeyContext = _apiKeyContextProvider.GetApiKeyContext();

            if (apiKeyContext == null)
                throw new EdFiSecurityException("No API key information was available for authorization.");

            return GetClaimsIdentity(apiKeyContext.EducationOrganizationIds, apiKeyContext.ClaimSetName, apiKeyContext.NamespacePrefix);
        }

        public ClaimsIdentity GetClaimsIdentity(IEnumerable<int> educationOrganizationIds, string claimSetName, string namespacePrefix)
        {
            var resourceClaims = _securityRepository.GetClaimsForClaimSet(claimSetName);

            // Group the resource claims by name to combine actions (and by claim set name if multiple claim sets are supported in the future)
            var resourceClaimsByClaimName =
                from c in resourceClaims
                group c by c.ResourceClaim.ClaimName
                into g
                select g;

            // Create a list of resource claims to be issued.
            var claims = (from grouping in resourceClaimsByClaimName
                          let claimValue = new EdFiResourceClaimValue
                          {
                              Actions = grouping.Select(x => x.Action.ActionUri).ToArray(), 
                              EducationOrganizationIds = educationOrganizationIds.ToList(),
                          }
                          select JsonClaimHelper.CreateClaim(grouping.Key, claimValue))
                          .ToList();

            // TODO: Needs unit test
            if (string.IsNullOrWhiteSpace(namespacePrefix))
                throw new Exception("Unable to assign NamespacePrefix claim because the value returned was null or empty.");

            // Create Identity Claims
            // NamespacePrefix
            claims.Add(new Claim(EdFiOdsApiClaimTypes.NamespacePrefix, namespacePrefix));
            
            return new ClaimsIdentity(claims, EdFiAuthenticationTypes.OAuth);
        }
    }
}
