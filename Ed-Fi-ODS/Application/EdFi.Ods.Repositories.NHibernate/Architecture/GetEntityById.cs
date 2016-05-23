using System;
using System.Linq;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.NHibernate.Architecture;
using NHibernate;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    /// <summary>
    /// Provides an implementation of the GetById method that works with NHibernate.
    /// </summary>
    /// <typeparam name="TEntity">The Type of the entity to retrieve.</typeparam>
    public class GetEntityById<TEntity> : NHibernateRepositoryOperationBase, IGetEntityById<TEntity> 
        where TEntity : DomainObjectBase, IHasIdentifier, IDateVersionedEntity
    {
        private readonly IGetEntitiesByIds<TEntity> _getEntitiesByIds;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetEntityById"/> class using the specified session factory and IGetEntitiesByIds implementation.
        /// </summary>
        /// <param name="sessionFactory">The NHibernate Session Factory.</param>
        /// <param name="getEntitiesByIds">The "GetByIds" implementation to which to delegate.</param>
        public GetEntityById(ISessionFactory sessionFactory, IGetEntitiesByIds<TEntity> getEntitiesByIds)
            : base(sessionFactory)
        {
            _getEntitiesByIds = getEntitiesByIds;
        }

        /// <summary>
        /// Gets a single entity by its unique identifier.
        /// </summary>
        /// <param name="id">The value of the unique identifier.</param>
        /// <returns>The specified entity if found; otherwise null.</returns>
        public TEntity GetById(Guid id)
        {
            return _getEntitiesByIds.GetByIds(new[] { id }).SingleOrDefault();
        }
    }
}