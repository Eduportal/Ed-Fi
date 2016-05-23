using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using EdFi.Ods.Entities.NHibernate.Mappings.SqlServer;
using EdFi.Ods.Entities.Repositories.NHibernate.Architecture;

namespace EdFi.Ods.BulkLoad.Services._Installers
{
    /// <summary>
    /// This class helps bootstrap dependencies for running the bulk load services
    /// </summary>
    public class XmlShreddingDatabaseInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            RegisterNHibernateAndDependencies(container);
        }

        private static void RegisterNHibernateAndDependencies(IWindsorContainer container)
        {
            var justToEnsureRefToAssembly = new Marker_EdFi_Ods_Entities_NHibernate_Mappings();

            (new NHibernateConfigurator()).Configure(container);
        }
    }
}