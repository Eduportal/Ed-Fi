using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.Common;
using EdFi.TestObjects;
using EdFi.TestObjects.Builders;
using TechTalk.SpecFlow;

namespace EdFi.Ods.WebService.Tests.Profiles
{
    public class FakeRepository<TEntity> : IGetEntityById<TEntity>, IUpsertEntity<TEntity>, 
        IGetEntitiesBySpecification<TEntity>
        where TEntity : IHasIdentifier, IDateVersionedEntity
    {
        private readonly ITestObjectFactory _factory;

        public IDictionary<Guid, TEntity> EntitiesById
        {
            get
            {
                string featureContextKey = typeof(TEntity).Name + "FakeRepositoryDictionary";

                IDictionary<Guid, TEntity> entitiesById;

                if (!FeatureContext.Current.TryGetValue(featureContextKey, out entitiesById))
                {
                    entitiesById = new Dictionary<Guid, TEntity>();
                    FeatureContext.Current.Set(entitiesById, featureContextKey);
                }

                return entitiesById;
            } 
        }

        public FakeRepository(ITestObjectFactory testObjectFactory)
        {
            _factory = testObjectFactory;
        }

        /// <summary>
        /// Gets a single entity by its unique identifier.
        /// </summary>
        /// <param name="id">The value of the unique identifier.</param>
        /// <returns>The specified entity if found; otherwise null.</returns>
        public TEntity GetById(Guid id)
        {
            TEntity value;

            if (EntitiesById.TryGetValue(id, out value))
                return value;

            var entity = _factory.Create<TEntity>();
            entity.Id = id;

            EntitiesById[entity.Id] = entity;
            return entity;
        }

        public TEntity Upsert(TEntity entity, bool enforceOptimisticLock, out bool isModified, out bool isCreated)
        {
            isModified = false;
            isCreated = false;

            TEntity existingEntity;

            // If the entity already exists, synchronize the entity the same way the real repository does
            if (EntitiesById.TryGetValue(entity.Id, out existingEntity))
            {
                (entity as ISynchronizable).Synchronize(existingEntity);
                isModified = true;
            }
            else
            {
                EntitiesById[entity.Id] = entity;
                isCreated = true;
            }

            return entity;
        }

        public IList<TEntity> GetBySpecification(TEntity specification, IQueryParameters queryParameters)
        {
            if (EntitiesById.Count == 0)
                GetById(Guid.NewGuid());

            return EntitiesById.Values.ToList();
        }
    }
}