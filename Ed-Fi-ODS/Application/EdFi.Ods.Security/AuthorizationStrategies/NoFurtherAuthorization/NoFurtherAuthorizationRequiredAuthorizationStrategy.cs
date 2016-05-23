using System.Collections.Generic;
using System.Security.Claims;
using EdFi.Common.Security.Authorization;
using EdFi.Common.Security.Claims;

namespace EdFi.Ods.Security.AuthorizationStrategies.NoFurtherAuthorization
{
    /// <summary>
    /// Implements an authorization strategy that performs no additional authorization.
    /// </summary>
    public class NoFurtherAuthorizationRequiredAuthorizationStrategy : IEdFiAuthorizationStrategy
    {
        public void AuthorizeSingleItem(IEnumerable<Claim> relevantClaims, EdFiAuthorizationContext authorizationContext)
        {
            // Note: all claim checks are done in the implementation of the IEdFiAuthorizationProvider.
            // Do nothing becuase the resource authorization metadata provider should have returned claims for the
            // requested action and the EdFi authorization provider should have validated. 
        }

        /// <summary>
        /// Applies filtering to a multiple-item request.
        /// </summary>
        /// <param name="relevantClaims">The subset of the caller's claims that are relevant for the authorization decision.</param>
        /// <param name="authorizationContext">The authorization context.</param>
        /// <param name="filterBuilder">A builder used to activate filters and assign parameter values.</param>
        /// <returns>The dictionary containing the filter information as appropriate, or <b>null</b> if no filters are required.</returns>
        public void ApplyAuthorizationFilters(IEnumerable<Claim> relevantClaims, EdFiAuthorizationContext authorizationContext, ParameterizedFilterBuilder filterBuilder)
        {
            // Note: all claim checks are done in the implementation of the IEdFiAuthorizationProvider.
            // Do nothing becuase the resource authorization metadata provider should have returned claims for the
            // requested action and the EdFi authorization provider should have validated. 
        }
    }
}
