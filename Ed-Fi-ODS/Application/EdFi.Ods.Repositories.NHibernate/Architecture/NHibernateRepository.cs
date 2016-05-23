using System;
using System.Collections.Generic;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;
using NHibernate;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    public class NHibernateRepository<TEntity>
        : IRepository<TEntity>,
        IGetEntitiesByIds<TEntity>,
        IUpsertEntity<TEntity>, 
        IGetEntitiesBySpecification<TEntity>,
        IDeleteEntityById<TEntity>, 
        IDeleteEntityByKey<TEntity>
        where TEntity : IHasIdentifier, IDateVersionedEntity
    {
        public ISessionFactory SessionFactory { get; private set; }

        private readonly IGetEntitiesByIds<TEntity> _getEntitiesByIds;
        private readonly IDeleteEntityById<TEntity> _deleteEntityById;
        private readonly IDeleteEntityByKey<TEntity> _deleteEntityByKey;
        private readonly IUpsertEntity<TEntity> _upsertEntity;
        private readonly IGetEntitiesBySpecification<TEntity> _getEntitiesBySpecification;

        public NHibernateRepository(ISessionFactory sessionFactory, 
            IGetEntitiesByIds<TEntity> getEntitiesByIds,
            IGetEntitiesBySpecification<TEntity> getEntitiesBySpecification,
            IUpsertEntity<TEntity> upsertEntity,
            IDeleteEntityById<TEntity> deleteEntityById,
            IDeleteEntityByKey<TEntity> deleteEntityByKey)
        {
            _getEntitiesByIds = getEntitiesByIds;
            _getEntitiesBySpecification = getEntitiesBySpecification;
            _upsertEntity = upsertEntity;
            _deleteEntityById = deleteEntityById;
            _deleteEntityByKey = deleteEntityByKey;
            
            SessionFactory = sessionFactory;
        }

        public IList<TEntity> GetByIds(IList<Guid> ids)
        {
            return _getEntitiesByIds.GetByIds(ids);
        }

        public IList<TEntity> GetBySpecification(TEntity specification, IQueryParameters queryParameters)
        {
            return _getEntitiesBySpecification.GetBySpecification(specification, queryParameters);
        }

        public TEntity Upsert(TEntity entity, bool enforceOptimisticLock, out bool isModified, out bool isCreated)
        {
            return _upsertEntity.Upsert(entity, enforceOptimisticLock, out isModified, out isCreated);
        }

        public void DeleteById(Guid id, string etag)
        {
            _deleteEntityById.DeleteById(id, etag);
        }

        public void DeleteByKey(TEntity specification, string etag)
        {
            _deleteEntityByKey.DeleteByKey(specification, etag);
        }
    }
}
