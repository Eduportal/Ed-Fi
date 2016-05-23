using System.Collections.Generic;
using System.Security.Claims;

namespace EdFi.Common.Security.Claims
{
    /// <summary>
    /// Defines a method for obtaining a <see cref="ClaimsIdentity"/> instance for building a <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public interface IClaimsIdentityProvider
    {
        /// <summary>
        /// Gets a <see cref="ClaimsIdentity"/> instance for building a <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <returns>The <see cref="ClaimsIdentity"/>.</returns>
        ClaimsIdentity GetClaimsIdentity();

        /// <summary>
        /// Gets a <see cref="ClaimsIdentity"/> instance for building a <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="localEducationAgencyIds">List of LocalEducationAgencyIds</param>
        /// <param name="claimSetName">Claim Set Name</param>
        /// <param name="vendorNamespacePrefix">Vendor Namespace Prefix</param>
        /// <returns>The <see cref="ClaimsIdentity"/>.</returns>
        ClaimsIdentity GetClaimsIdentity(IEnumerable<int> localEducationAgencyIds, string claimSetName, string vendorNamespacePrefix);
    }

    /// <summary>
    /// Defines authentication types supported by the Ed-Fi solution.
    /// </summary>
    public class EdFiAuthenticationTypes
    {
        /// <summary>
        /// The authentication type associated with the Ed-Fi OAuth implementation (which is applied to the <see cref="ClaimsIdentity"/> when it's created).
        /// </summary>
        public const string OAuth = "OAuth";
    }
}
