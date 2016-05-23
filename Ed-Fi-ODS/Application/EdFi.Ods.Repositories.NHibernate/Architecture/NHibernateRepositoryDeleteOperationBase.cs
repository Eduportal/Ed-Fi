using System;
using EdFi.Ods.Api.Common;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Exceptions;
using EdFi.Ods.Entities.NHibernate.Architecture;
using NHibernate;
using NHibernate.Persister.Entity;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    public class NHibernateRepositoryDeleteOperationBase<TEntity> 
        : NHibernateRepositoryOperationBase 
        where TEntity : DomainObjectBase, IHasIdentifier, IDateVersionedEntity
    {
        private readonly IETagProvider _eTagProvider;

        public NHibernateRepositoryDeleteOperationBase(ISessionFactory sessionFactory, IETagProvider eTagProvider) : base(sessionFactory)
        {
            _eTagProvider = eTagProvider;
        }

        protected void Delete(TEntity persistedEntity, string etag)
        {
            if (persistedEntity == null)
                throw new NotFoundException("Resource to delete was not found.");

            // only check last modified data
            if (!string.IsNullOrEmpty(etag))
            {
                var lastModifiedDate = _eTagProvider.GetDateTime(etag);
                if (!persistedEntity.LastModifiedDate.Equals(lastModifiedDate))
                    throw new ConcurrencyException("Resource was modified by another consumer.");
            }

            // Delete the incoming entity directly against the database using ADO.NET (rather than via NHibernate).
            using (var trans = Session.BeginTransaction())
            {
                try
                {
                    var classMetadata = (AbstractEntityPersister) Session.SessionFactory.GetClassMetadata(typeof(TEntity));
                    
                    var conn = Session.Connection;

                    using (var cmd = conn.CreateCommand())
                    {
                        Session.Transaction.Enlist(cmd);

                        if (classMetadata.IsInherited)
                            cmd.CommandText = "DELETE FROM " + classMetadata.RootTableName + " WHERE Id = '" + persistedEntity.Id + "'";
                        else
                            cmd.CommandText = "DELETE FROM " + classMetadata.TableName + " WHERE Id = '" + persistedEntity.Id + "'";

                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
    }
}