using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.Common.Validation;
using EdFi.Ods.Entities.NHibernate.Architecture;
using EdFi.Ods.Pipelines.Steps;
using NHibernate;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    public class UpdateEntity<TEntity> : ValidatingNHibernateRepositoryOperationBase, IUpdateEntity<TEntity> 
        where TEntity : DomainObjectBase, IHasIdentifier, IDateVersionedEntity
    {
        public UpdateEntity(ISessionFactory sessionFactory, IEnumerable<IObjectValidator> validators)
            : base(sessionFactory, validators)
        {
        }

        public void Update(TEntity persistentEntity)
        {
            ValidateEntity(persistentEntity);

            using (var trans = Session.BeginTransaction())
            {
                try
                {
                    Session.Update(persistentEntity);
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
                finally
                {
                    if (!trans.WasRolledBack)
                        trans.Commit();
                }
            }            
        }
    }
}