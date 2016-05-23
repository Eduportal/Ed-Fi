using System;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Exceptions;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.NHibernate.Architecture;
using EdFi.Ods.Api.Common;
using NHibernate;
using NHibernate.Context;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    public class DeleteEntityByKey<TEntity> : NHibernateRepositoryDeleteOperationBase<TEntity>, IDeleteEntityByKey<TEntity>
        where TEntity : DomainObjectBase, IHasIdentifier, IDateVersionedEntity
    {
        private readonly IGetEntityByKey<TEntity> _getEntityByKey;

        public DeleteEntityByKey(ISessionFactory sessionFactory, 
            IGetEntityByKey<TEntity> getEntityByKey, 
            IETagProvider eTagProvider)
            : base(sessionFactory, eTagProvider)
        {
            _getEntityByKey = getEntityByKey;
        }

        public void DeleteByKey(TEntity specification, string etag)
        {
            var shouldReleaseBind = EnsureSessionContextBinding();

            try
            {
                // First we must load the entity
                var persistedEntity = _getEntityByKey.GetByKey(specification);

                Delete(persistedEntity, etag);
            }
            finally
            {
                if (shouldReleaseBind)
                    CurrentSessionContext.Unbind(SessionFactory);
            }
        }
    }
}