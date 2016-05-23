using System;

namespace EdFi.Ods.Api.Common.Authorization
{
    /// <summary>
    /// Defines a method for obtaining the API client's details from their OAuth security token.
    /// </summary>
    public interface IOAuthTokenValidator
    {
        /// <summary>
        /// Gets the API client details for the supplied OAuth security token.
        /// </summary>
        /// <param name="token">The OAuth security token.</param>
        /// <returns>The <see cref="ApiClientDetails"/> associatd with the token.</returns>
        ApiClientDetails GetClientDetailsForToken(Guid token);
    }
}
