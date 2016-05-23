using System;
using System.Collections.Generic;

namespace EdFi.Ods.Common.Repositories
{
    public interface IRepository<TEntity>
        where TEntity : IHasIdentifier
    {
        /// <summary>
        ///     Gets an aggregate by it's unique identifier (either a natural key or assigned GUID).
        /// </summary>
        /// <param name="ids">The unique identifiers of the aggregate root entity to retrieve.</param>
        /// <returns>The entire domain aggregate.</returns>
        IList<TEntity> GetByIds(IList<Guid> ids);

        /// <summary>
        ///     Gets multiple aggregate roots by a combination of "query by example" and/or advanced criteria and paging parameters.
        /// </summary>
        /// <param name="specification">An instance of the aggregate root entity type, representing exact match search criteria, or null if not used.</param>
        /// <param name="queryParameters">Addtional search criteria.</param>
        /// <returns>A list of aggregates matching the specified criteria.</returns>
        IList<TEntity> GetBySpecification(TEntity specification, IQueryParameters queryParameters);


        /// <summary>
        ///     Updates an existing aggregate.
        /// </summary>
        /// <param name="entity">The aggregate root of the aggregate to be updated.</param>
        /// <param name="enforceOptimisticLock"></param>
        /// <param name="isModified">Indicates whether an existing entity was updated.</param>
        /// <param name="isCreated">Indicates whether a new entity was created.</param>
        /// <returns>The persisted entity.</returns>
        TEntity Upsert(TEntity entity, bool enforceOptimisticLock, out bool isModified, out bool isCreated);

        /// <summary>
        ///     Loads and then deletes the aggregate given the identifier and the last modified date (for optimistic locking purposes).
        /// </summary>
        /// <param name="id">The unique identifier of the aggregate root entity.</param>
        /// <param name="etag">The last known etag value of the resource to be deleted.</param>
        void DeleteById(Guid id, string etag);

        /// <summary>
        ///     Loads and then deletes the aggregate given the identifier and the last modified date (for optimistic locking purposes).
        /// </summary>
        /// <param name="specification">The specification containing the primary key values of the entity to delete.</param>
        /// <param name="etag">The last known etag value of the resource to be deleted.</param>
        void DeleteByKey(TEntity specification, string etag);
    }
}