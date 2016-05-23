using System;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.NHibernate.Architecture;
using EdFi.Ods.Api.Common;
using NHibernate;
using NHibernate.Context;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    public class DeleteEntityById<TEntity> 
        : NHibernateRepositoryDeleteOperationBase<TEntity>, IDeleteEntityById<TEntity> 
        where TEntity : DomainObjectBase, IHasIdentifier, IDateVersionedEntity
    {
        private readonly IGetEntityById<TEntity> _getEntityById;

        public DeleteEntityById(ISessionFactory sessionFactory, IGetEntityById<TEntity> getEntityById, IETagProvider eTagProvider)
            : base(sessionFactory, eTagProvider)
        {
            _getEntityById = getEntityById;
        }

        public void DeleteById(Guid id, string etag)
        {
            var shouldReleaseBind = EnsureSessionContextBinding();

            try
            {
                // First we must load the entity (this call is also used by the authorization decorators
                // to authorize according to the value returned by the GetById method).
                var persistedEntity = _getEntityById.GetById(id);

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