using System;
using System.Collections.Generic;
using EdFi.Common;
using EdFi.Common.Extensions;
using EdFi.Ods.Common;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.NHibernate.Architecture;
using NHibernate;
using NHibernate.Id;
using NHibernate.Persister.Entity;
using ObjectNotFoundException = System.Data.ObjectNotFoundException;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    public class CreateEntity<TEntity> : ValidatingNHibernateRepositoryOperationBase, ICreateEntity<TEntity> 
        where TEntity : DomainObjectBase, IHasIdentifier, IDateVersionedEntity
    {
        public CreateEntity(ISessionFactory sessionFactory, IEnumerable<IObjectValidator> validators)
            : base(sessionFactory, validators)
        {
        }

        public void Create(TEntity entity, bool enforceOptimisticLock)
        {
            var metadata = (SessionFactory.GetClassMetadata(typeof(TEntity)));

            // Does the entity have assignable, single-valued Id?
            if (metadata.HasIdentifierProperty)
            {
                var persister = metadata as AbstractEntityPersister;

                bool hasAssignableIdentifier = (metadata.HasIdentifierProperty && persister != null &&
                                                persister.IdentifierGenerator is Assigned);

                var identifierValue = metadata.GetIdentifier(entity, EntityMode.Poco);
                var identifierDefaultValue = metadata.IdentifierType.ReturnedClass.GetDefaultValue();

                bool identifierValueAssigned = !identifierValue.Equals(identifierDefaultValue);

                // If Id is assignable...
                if (hasAssignableIdentifier)
                {
                    // Make sure identifier has been assigned
                    if (!identifierValueAssigned)
                        throw new ArgumentException(
                            string.Format("Value for resource's identifier property '{0}' is required.",
                                          metadata.IdentifierPropertyName));
                }
                else
                {
                    // Make sure identifier has NOT been assigned
                    if (identifierValueAssigned)
                        throw new ArgumentException(
                            string.Format(
                                "Value for the auto-assigned identifier property '{0}' cannot be assigned by the client (value was '{1}'.",
                                metadata.IdentifierPropertyName, identifierValue));
                }
            }
            else
            {
                // Do we have an 'Id' value present?
                bool idHasValue = IdHasValue(entity);

                // The primary key is a composite key, so the Id must be a GUID which is client-assignable
                // However, if the client indicated we should enforce optimistic locking and provided an etag
                // value, this indicates that the resource has been deleted by another process and should not 
                // be recreated.  Instead, an exception should be thrown.
                if (idHasValue && enforceOptimisticLock)
                    throw new ObjectNotFoundException(
                        string.Format(
                            "Aggregate identified by '{0}' could not be found for update (it may have been deleted by another consumer).",
                            entity.Id));

                // New GUID identifiers are assigned by the NHibernate IPreInsertEventListener implementation
            }

            ValidateEntity(entity);

            // Save the incoming entity
            using (var trans = Session.BeginTransaction())
            {
                try
                {
                    Session.Save(entity);
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
                finally
                {
                    if (!trans.WasRolledBack)
                    {
                        trans.Commit();
                    }
                }
            }
        }

        private static bool IdHasValue(TEntity entity)
        {
            return !entity.Id.Equals(default(Guid));
        }
    }
}