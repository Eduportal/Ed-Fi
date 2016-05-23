using Castle.Windsor;
using EdFi.Ods.Common._Installers;
using EdFi.Ods.Common._Installers.ComponentNaming;

namespace EdFi.Ods.Repositories.NHibernate.Tests
{
    public class DatabaseConnectionStringProvidersInstaller : CommonDatabaseConnectionStringProvidersInstallerBase
    {
        protected override void RegisterOdsDatabaseConnectionStringProvider(IWindsorContainer container)
        {
            RegisterNamedConnectionStringProvider(container, Databases.Ods);
        }

        protected override void RegisterUniqueIdIntegratioDatabaseConnectionStringProvider(IWindsorContainer container)
        {
            RegisterNamedConnectionStringProvider(container, Databases.UniqueIdIntegration);
        }
    }
}
