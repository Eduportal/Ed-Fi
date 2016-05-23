namespace EdFi.Ods.Entities.Common.UniqueIdIntegration
{
    using System;

    /// <summary>
    /// Defines a method that creates a persistent mapping between a supplied UniqueId 
    /// value and a GUID-based identifier.
    /// </summary>
    public interface IUniqueIdPersonMappingFactory
    {
        /// <summary>
        /// Creates a persistent mapping between the supplied UniqueId value and GUID-based identifier.
        /// </summary>
        /// <param name="uniqueId">The UniqueId of the person.</param>
        /// <param name="id">The associated GUID-based identifier.</param>
        void CreateMapping(string uniqueId, Guid id);
    }
}