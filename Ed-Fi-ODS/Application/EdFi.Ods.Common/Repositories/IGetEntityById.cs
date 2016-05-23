using System;

namespace EdFi.Ods.Common.Repositories
{
    /// <summary>
    /// Defines a method for retrieving and entity by its identifier.
    /// </summary>
    /// <typeparam name="TEntity">The Type of the entity to be retrieved.</typeparam>
    public interface IGetEntityById<out TEntity> 
        where TEntity : IHasIdentifier, IDateVersionedEntity
    {
        /// <summary>
        /// Gets a single entity by its unique identifier.
        /// </summary>
        /// <param name="id">The value of the unique identifier.</param>
        /// <returns>The specified entity if found; otherwise null.</returns>
        TEntity GetById(Guid id);
    }
}