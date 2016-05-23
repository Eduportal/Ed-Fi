using System;

namespace EdFi.Ods.Entities.Common.Caching
{
    /// <summary>
    /// Defines methods for obtaining the UniqueIds corresponding to Id values (or vice-versa) for a person.
    /// </summary>
    public interface IPersonUniqueIdToIdCache
    {
        /// <summary>
        /// Gets the GUID-based identifier for the specified type of person and their UniqueId value.
        /// </summary>
        /// <param name="personTypeName">The type of the person (e.g. Staff, Student, Parent).</param>
        /// <param name="uniqueId">The UniqueId value associated with the person.</param>
        /// <returns>The GUID-based identifier for the person; otherwise the default GUID value.</returns>
        Guid GetId(string personTypeName, string uniqueId);

        /// <summary>
        /// Gets the externally defined UniqueId for the specified type of person and the GUID-based identifier.
        /// </summary>
        /// <param name="personTypeName">The type of the person (e.g. Staff, Student, Parent).</param>
        /// <param name="id">The GUID-based identifier for the person.</param>
        /// <returns>The UniqueId value assigned to the person if found; otherwise <b>null</b>.</returns>
        string GetUniqueId(string personTypeName, Guid id);
    }
}