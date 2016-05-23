namespace EdFi.Ods.Utilities.LoadGeneration.Security
{
    /// <summary>
    /// Defines methods for getting and setting REST API security context.
    /// </summary>
    public interface IApiSecurityContextProvider
    {
        /// <summary>
        /// Gets the current REST API client's security context.
        /// </summary>
        /// <returns>An <see cref="ApiSecurityContext"/> instance containing pertinent security information.</returns>
        ApiSecurityContext GetSecurityContext();

        /// <summary>
        /// Sets the current security context for making calls to the REST API.
        /// </summary>
        /// <param name="apiSecurityContext">The <see cref="ApiSecurityContext"/> instance containing pertinent security information.</param>
        void SetSecurityContext(ApiSecurityContext apiSecurityContext);
    }
}