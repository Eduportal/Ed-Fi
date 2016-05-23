namespace EdFi.Ods.Entities.Common.Caching
{
    /// <summary>
    /// Defines methods for obtaining the UniqueIds corresponding to USI values (or vice-versa) for a person.
    /// </summary>
    public interface IPersonUniqueIdToUsiCache
    {
        /// <summary>
        /// Gets the externally defined UniqueId for the specified type of person and the ODS-specific surrogate identifier.
        /// </summary>
        /// <param name="personTypeName">The type of the person (e.g. Staff, Student, Parent).</param>
        /// <param name="usi">The integer-based identifier for the specified representation of the person, 
        /// specific to a particular ODS database instance.</param>
        /// <returns>The UniqueId value assigned to the person if found; otherwise <b>null</b>.</returns>
        string GetUniqueId(string personTypeName, int usi);

        /// <summary>
        /// Gets the ODS-specific integer identifier for the specified type of person and their UniqueId value.
        /// </summary>
        /// <param name="personTypeName">The type of the person (e.g. Staff, Student, Parent).</param>
        /// <param name="uniqueId">The UniqueId value associated with the person.</param>
        /// <returns>The ODS-specific integer identifier for the specified type of representation of 
        /// the person if found; otherwise 0.</returns>
        int GetUsi(string personTypeName, string uniqueId);

        /// <summary>
        /// Gets the ODS-specific integer identifier for the specified type of person and their UniqueId value.
        /// </summary>
        /// <param name="personTypeName">The type of the person (e.g. Staff, Student, Parent).</param>
        /// <param name="uniqueId">The UniqueId value associated with the person.</param>
        /// <returns>The ODS-specific integer identifier for the specified type of representation of 
        /// the person if found; otherwise <b>null</b>.</returns>
        int? GetUsiNullable(string personTypeName, string uniqueId);
    }
}