using System;
using System.Collections.Generic;

namespace EdFi.Ods.Common.Repositories
{
    /// <summary>
    /// Defines a method for retrieving a list of entities by their identifiers.
    /// </summary>
    /// <typeparam name="TEntity">The Type of the entities to be retrieved.</typeparam>
    public interface IGetEntitiesByIds<TEntity> 
        where TEntity : IHasIdentifier, IDateVersionedEntity 
    {   
        /// <summary>
        /// Get a list of entities by their identifiers.
        /// </summary>
        /// <param name="ids">The list of identifiers.</param>
        /// <returns>The list of matching entities.</returns>
        IList<TEntity> GetByIds(IList<Guid> ids);
    }
}