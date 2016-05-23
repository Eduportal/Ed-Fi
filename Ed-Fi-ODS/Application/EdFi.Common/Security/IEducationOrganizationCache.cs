namespace EdFi.Common.Security
{
    /// <summary>
    /// Defines a method for obtaining all the education organizations identifiers 
    /// associated with a specifiec education organization, by identifier.
    /// </summary>
    public interface IEducationOrganizationCache
    {
        /// <summary>
        /// Finds the <see cref="EducationOrganizationIdentifiers"/> for the specified <paramref name="educationOrganizationId"/>, or <b>null</b>.
        /// </summary>
        /// <param name="educationOrganizationId">The generic Education Organization identifier for which to search.</param>
        /// <returns>The matching <see cref="EducationOrganizationIdentifiers"/>; otherwise <b>null</b>.</returns>
        EducationOrganizationIdentifiers GetEducationOrganizationIdentifiers(int educationOrganizationId);
        
        // TODO: GKM - Rather than return null, convert to use Try semantics?
        // TODO: GKM - For EdOrg extensibility, we will want to make this interface generic, like the services on the dashboards.
    }
}
