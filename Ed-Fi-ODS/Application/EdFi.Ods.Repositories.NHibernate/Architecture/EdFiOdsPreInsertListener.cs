using System;
using EdFi.Ods.Entities.NHibernate.Architecture;
using NHibernate.Event;
using NHibernate.Persister.Entity;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    public class EdFiOdsPreInsertListener : IPreInsertEventListener
    {
        public bool OnPreInsert(PreInsertEvent @event)
        {
            Set(@event.Persister, @event.State, "CreateDate", DateTime.Now);

            int idIndex = GetPropertyIndex(@event.Persister, "Id");

            if (idIndex >= 0 && @event.Persister.GetPropertyType("Id").ReturnedClass == typeof(Guid))
            {
                // Create a new Guid if one was not assigned by client
                if (Get<Guid>(@event.Persister, @event.State, "Id") == default(Guid))
                {
                    Guid newGuid = Guid.NewGuid();
                    Set(@event.Persister, @event.State, "Id", newGuid);

                    // TODO: Need to be verified that this step provides the aggregate's Id
                    // to derived classes that about to be inserted (e.g. EdOrg -> School).
                    // The derived class should have the same Id value in the table.
                    var aggregateRoot = @event.Entity as AggregateRootWithCompositeKey;

                    if (aggregateRoot != null)
                        aggregateRoot.Id = newGuid;
                }
            }

            return false;
        }

        private int GetPropertyIndex(IEntityPersister persister, string propertyName)
        {
            return Array.IndexOf(persister.PropertyNames, propertyName);
        }

        private void Set(object[] state, int propertyIndex, object value)
        {
            if (propertyIndex == -1)
                return;

            state[propertyIndex] = value;
        }

        private void Set(IEntityPersister persister, object[] state, string propertyName, object value)
        {
            var propertyIndex = Array.IndexOf(persister.PropertyNames, propertyName);
            Set(state, propertyIndex, value);
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