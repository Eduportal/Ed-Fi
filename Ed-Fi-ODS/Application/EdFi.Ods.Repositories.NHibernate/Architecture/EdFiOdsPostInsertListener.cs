using System;
using EdFi.Ods.Entities.NHibernate.Architecture;
using NHibernate.Event;
using NHibernate.Persister.Entity;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    public class EdFiOdsPostInsertListener : IPostInsertEventListener
    {
        public void OnPostInsert(PostInsertEvent @event)
        {
            var domainEntity = @event.Entity as DomainObjectBase;

            if (domainEntity == null)
                return;

            DateTime createDateValue = Get<DateTime>(@event.Persister, @event.State, "CreateDate");

            if (!createDateValue.Equals(default(DateTime)))
                domainEntity.CreateDate = createDateValue;

            var aggregateRoot = @event.Entity as AggregateRootWithCompositeKey;

            if (aggregateRoot != null)
            {
                // Assign the server-assigned Id back to the aggregate root entity
                if (aggregateRoot.Id.Equals(Guid.Empty))
                    aggregateRoot.Id = Get<Guid>(@event.Persister, @event.State, "Id");
            }
        }

        private T Get<T>(IEntityPersister persister, object[] state, string propertyName)
        {
            var index = Array.IndexOf(persister.PropertyNames, propertyName);

            if (index == -1)
                return default(T);

            return (T) state[index];
        }
    }
}