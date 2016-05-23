using EdFi.Common.Security.Authorization;

namespace EdFi.Common.Security.Claims
{
    /// <summary>
    /// Defines methods for performing single-item and filter-based authorization appropriate to the claims, resource, action and possibly the entity instance supplied in the <see cref="EdFiAuthorizationContext"/>.
    /// </summary>
    public interface IEdFiAuthorizationProvider
    {
        /// <summary>
        /// Authorizes a single-item request using the claims, resource, action and entity instance supplied in the <see cref="EdFiAuthorizationContext"/>.
        /// </summary>
        /// <param name="authorizationContext">The authorization context to be used in making the authorization decision.</param>
        void AuthorizeSingleItem(EdFiAuthorizationContext authorizationContext);

        /// <summary>
        /// Authorizes a multiple-item read request using the claims, resource, action and entity instance supplied in the <see cref="EdFiAuthorizationContext"/>.
        /// </summary>
        /// <param name="authorizationContext">The authorization context to be used in making the authorization decision.</param>
        /// <param name="filterBuilder">A builder used to activate filters and assign parameter values.</param>
        /// <returns></returns>
        void ApplyAuthorizationFilters(EdFiAuthorizationContext authorizationContext, ParameterizedFilterBuilder filterBuilder);
    }
}