namespace EdFi.Ods.Tests.EdFi.Ods.Common._Stubs.Repositories
{
    using System;
    using System.Collections.Generic;

    using global::EdFi.Ods.Common;
    using global::EdFi.Ods.Common.Repositories;

    public class StubRepositoryBuilder<TEntity>
        where TEntity : IHasIdentifier, IDateVersionedEntity
    {
        private readonly StubRepository<TEntity> _instance = new StubRepository<TEntity>();

        public static implicit operator StubRepository<TEntity>(StubRepositoryBuilder<TEntity> builder)
        {
            return builder._instance;
        }

        public StubRepositoryBuilder<TEntity> ResourceIsAlwaysModified
        {
            get
            {
                this._instance.StubIsModified(true);
                return this;
            }
        }

        public StubRepositoryBuilder<TEntity> ResourceIsAlwaysCreated
        {
            get
            {
                this._instance.StubIsCreated(true);
                return this;
            }
        }

        public StubRepositoryBuilder<TEntity> ResourceIsNeverCreatedOrModified
        {
            get
            {
                this._instance.StubIsCreated(false);
                this._instance.StubIsModified(false);
                return this;
            }
        }

        public StubRepositoryBuilder<TEntity> OnUpsertThrow(Exception e)
        {
            this._instance.ExceptionToThrowOnUpsert = e;
            return this;
        }
    }

    public class StubRepository<TEntity> : IUpsertEntity<TEntity>
        where TEntity : IHasIdentifier, IDateVersionedEntity
    {
        private bool _stubIsModified;
        private bool _stubIsCreated;

        public Exception ExceptionToThrowOnUpsert { get; set; }

        public IList<TEntity> GetByIds(IList<Guid> ids)
        {
            throw new NotImplementedException();
        }

        public IList<TEntity> GetBySpecification(TEntity specification, IQueryParameters queryParameters)
        {
            throw new NotImplementedException();
        }

        public TEntity Upsert(TEntity entity, bool enforceOptimisticLock, out bool isModified, out bool isCreated)
        {
            if (this.ExceptionToThrowOnUpsert != null)
                throw this.ExceptionToThrowOnUpsert;

            isModified = this._stubIsModified;
            isCreated = this._stubIsCreated;
            return entity;
        }

        public void DeleteById(Guid id, string etag)
        {
            throw new NotImplementedException();
        }

        public void DeleteByKey(TEntity specification, string etag)
        {
            throw new NotImplementedException();
        }

        public void StubIsModified(bool value)
        {
            this._stubIsModified = value;
        }

        public void StubIsCreated(bool value)
        {
            this._stubIsCreated = value;
        }
    }
}