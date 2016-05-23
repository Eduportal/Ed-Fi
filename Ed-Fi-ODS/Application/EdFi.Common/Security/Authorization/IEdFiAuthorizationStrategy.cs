using System.Collections.Generic;
using System.Security.Claims;
using EdFi.Common.Security.Claims;

namespace EdFi.Common.Security.Authorization
{
    /// <summary>
    /// Defines methods for authorizing both single-item requests, and multiple item requests (requiring filtering).
    /// </summary>
    public interface IEdFiAuthorizationStrategy 
    {
        /// <summary>
        /// Authorize the request for a single item.
        /// </summary>
        /// <param name="relevantClaims">The subset of the caller's claims that are relevant for the authorization decision.</param>
        /// <param name="authorizationContext">The authorization context, including the entity's data.</param>
        void AuthorizeSingleItem(IEnumerable<Claim> relevantClaims, EdFiAuthorizationContext authorizationContext);

        /// <summary>
        /// Applies filtering to a multiple-item request.
        /// </summary>
        /// <param name="relevantClaims">The subset of the caller's claims that are relevant for the authorization decision.</param>
        /// <param name="authorizationContext">The authorization context.</param>
        /// <param name="filterBuilder">A builder used to activate filters and assign parameter values.</param>
        /// <returns>The dictionary containing the filter information as appropriate, or <b>null</b> if no filters are required.</returns>
        void ApplyAuthorizationFilters(IEnumerable<Claim> relevantClaims, EdFiAuthorizationContext authorizationContext, ParameterizedFilterBuilder filterBuilder);
    }
}
