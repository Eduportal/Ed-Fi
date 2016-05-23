using System;
using EdFi.Ods.Entities.Common.Caching;

namespace EdFi.Ods.Entities.Common.IdentityValueMappers
{
    /// <summary>
    /// Provides interfaces for mapping between a UniqueId and the Id (i.e. the API resource "id").
    /// </summary>
    /// <remarks>
    /// Implementors of this interface should return <see cref="PersonIdentifiersValueMap"/> instances containing at least the
    /// value being requested on each method.  For optimization purposes, they may also return the tertiary identification
    /// value which will then be opportunistically cached by the <see cref="PersonIdentifiersCache"/> component (and in an
    /// ODS-specific manner for USI values).
    /// 
    /// If the requested value cannot be found, then a default instance of the <see cref="PersonIdentifiersValueMap"/> should be returned.
    /// </remarks>
    public interface IUniqueIdToIdValueMapper
    {
        /// <summary>
        /// Gets the identifier values for a given uniqueId value (only guaranteeing the return of the Id and only if it's found).
        /// </summary>
        /// <param name="personType">The type of person whose Id is being requested.</param>
        /// <param name="uniqueId">The uniqueId of the person whose Id is being requested.</param>
        /// <returns>The <see cref="PersonIdentifiersValueMap"/> containing the requested Id (if found), and possibly the 
        /// corresponding Usi (depending on the implementation); otherwise a <see cref="PersonIdentifiersValueMap"/> instance
        /// containing default values.</returns>
        PersonIdentifiersValueMap GetId(string personType, string uniqueId);

        /// <summary>
        /// Gets the identifier values for a given resource Id value (only guaranteeing the return of the UniqueId and only if it's found).
        /// </summary>
        /// <param name="personType">The type of person whose UniqueId is being requested.</param>
        /// <param name="id">The resource Id of the person whose UniqueId is being requested.</param>
        /// <returns>The <see cref="PersonIdentifiersValueMap"/> containing the requested UniqueId (if found), and possibly the 
        /// corresponding Usi (depending on the implementation); otherwise a <see cref="PersonIdentifiersValueMap"/> instance
        /// containing default values.</returns>
        PersonIdentifiersValueMap GetUniqueId(string personType, Guid id);
    }
}