using NHibernate.Cfg;
using NHibernate.Event;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    public static class NHibernateConfigurationExtensions
    {
        public static void AddCreateDateHooks(this Configuration configuration)
        {
            configuration.Interceptor = new CreateDateBasedTransientInterceptor();
            configuration.SetListener(ListenerType.PreInsert, new EdFiOdsPreInsertListener());
            configuration.SetListener(ListenerType.PostInsert, new EdFiOdsPostInsertListener());
            configuration.SetListener(ListenerType.PostUpdate, new EdFiOdsPostUpdateEventListener());
            //configuration.SetListener(ListenerType.PreUpdate, new PreUpdateEventListener());
        }
    }
}