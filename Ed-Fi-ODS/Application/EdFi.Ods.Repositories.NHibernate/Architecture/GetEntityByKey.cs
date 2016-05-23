using System;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.NHibernate.Architecture;
using NHibernate;
using NHibernate.Context;
using NHibernate.Criterion;
using NHibernate.Util;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    public class GetEntityByKey<TEntity> : NHibernateRepositoryOperationBase, IGetEntityByKey<TEntity> 
        where TEntity : DomainObjectBase, IDateVersionedEntity, IHasIdentifier
    {
        public GetEntityByKey(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        /// <summary>
        /// Gets a single entity by its composite primary key values.
        /// </summary>
        /// <param name="specification">An entity instance that has all the primary key properties assigned with values.</param>
        /// <returns>The specified entity if found; otherwise null.</returns>
        public TEntity GetByKey(TEntity specification)
        {
            var shouldReleaseBind = EnsureSessionContextBinding();

            TEntity persistedEntity = null;

            try
            {
                // Look up by composite key
                var entityWithKeyValues = specification as IHasPrimaryKeyValues;

                if (entityWithKeyValues == null)
                    throw new ArgumentException("The '{0}' entity does not support accessing primary key values.");

                // Go try to get the existing entity
                var compositeKeyValues = entityWithKeyValues.GetPrimaryKeyValues();

                // Only look up by composite key if "Id" is not considered part of the "DomainSignature"
                if (!compositeKeyValues.Contains("Id"))
                {
                    ICriteria c = SessionFactory.GetCurrentSession().CreateCriteria<TEntity>();
                    c.Add(Restrictions.AllEq(compositeKeyValues));
                    // ************
                    // Cannot use SetMaxResult as it causes 'select top n' sql statement which conflicts with 
                    // getbyspecification interceptor (see CreateDateBasedTransientAndAuthorizationInjectionInterceptor)
                    // *************
                    persistedEntity = c.UniqueResult<TEntity>();
                }

                if (persistedEntity != null)
                    return persistedEntity;


                // Look up by unique keys
                var entityWithUniqueValues = specification as IHasNonPrimaryKeyUniqueValues;
                if (entityWithUniqueValues == null)
                    return persistedEntity;

                var nonPrimaryUniqueKeyValues = entityWithUniqueValues.GetNonPrimaryUniqueKeyValues();
                if (nonPrimaryUniqueKeyValues.Any())
                {
                    ICriteria c = SessionFactory.GetCurrentSession().CreateCriteria<TEntity>();
                    c.Add(Restrictions.AllEq(nonPrimaryUniqueKeyValues));
                    persistedEntity = c.UniqueResult<TEntity>();
                }

                return persistedEntity;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (shouldReleaseBind)
                    CurrentSessionContext.Unbind(SessionFactory);
            }
        }
    }
}