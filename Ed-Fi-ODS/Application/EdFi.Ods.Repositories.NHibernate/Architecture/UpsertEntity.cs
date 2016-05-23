using System;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Exceptions;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.Common;
using EdFi.Ods.Entities.NHibernate.Architecture;
using NHibernate;
using NHibernate.Context;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    public class UpsertEntity<TEntity> : NHibernateRepositoryOperationBase, IUpsertEntity<TEntity>
        where TEntity : DomainObjectBase, IHasIdentifier, IDateVersionedEntity, ISynchronizable
    {
        private readonly IGetEntityById<TEntity> _getEntityById;
        private readonly IGetEntityByKey<TEntity> _getEntityByKey;
        private readonly ICreateEntity<TEntity> _createEntity;
        private readonly IUpdateEntity<TEntity> _updateEntity;

        public UpsertEntity(ISessionFactory sessionFactory, 
            IGetEntityById<TEntity> getEntityById, 
            IGetEntityByKey<TEntity> getEntityByKey,
            ICreateEntity<TEntity> createEntity,
            IUpdateEntity<TEntity> updateEntity) : base(sessionFactory)
        {
            _getEntityById = getEntityById;
            _getEntityByKey = getEntityByKey;
            _createEntity = createEntity;
            _updateEntity = updateEntity;
        }

        public TEntity Upsert(TEntity entity, bool enforceOptimisticLock, out bool isModified, out bool isCreated)
        {
            var shouldReleaseBind = EnsureSessionContextBinding();

            try
            {
                isCreated = false;
                isModified = false;

                // Go try to get the existing entity
                TEntity persistedEntity = null;

                // Do we have an 'Id' value present?
                bool idHasValue = !entity.Id.Equals(default(Guid));

                if (idHasValue)
                {
                    // Look up by provided Id
                    persistedEntity = _getEntityById.GetById(entity.Id);
                }
                else
                {
                    // Get it by primary key
                    persistedEntity = _getEntityByKey.GetByKey(entity);
                }

                // If there is no existing entity...
                if (persistedEntity == null)
                {
                    // Create the entity
                    _createEntity.Create(entity, enforceOptimisticLock);
                    persistedEntity = entity;
                    isCreated = true;
                }
                else
                {
                    // Update the entity
                    if (enforceOptimisticLock)
                    {
                        if (!persistedEntity.LastModifiedDate.Equals(entity.LastModifiedDate))
                            throw new ConcurrencyException("Resource was modified by another consumer.");
                    }

                    // Synchronize using strongly-typed generated code
                    isModified = entity.Synchronize(persistedEntity);

                    // Force aggregate root to be touched with an updated date if aggregate has been modified
                    if (isModified)
                    {
                        // Make root dirty, NHibernate will override the value during insert (through a hook)
                        persistedEntity.CreateDate = persistedEntity.CreateDate.AddSeconds(1);
                    }

                    _updateEntity.Update(persistedEntity);
                }

                return persistedEntity;
            }
            finally
            {
                if (shouldReleaseBind)
                    CurrentSessionContext.Unbind(SessionFactory);
            }
        }
    }
}