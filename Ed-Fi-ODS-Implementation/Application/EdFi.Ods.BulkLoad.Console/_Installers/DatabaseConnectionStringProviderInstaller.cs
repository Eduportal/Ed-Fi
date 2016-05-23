using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Database;
using EdFi.Ods.Common._Installers;
using EdFi.Ods.Common._Installers.ComponentNaming;

namespace EdFi.Ods.BulkLoad.Console._Installers
{
    public class DatabaseConnectionStringProviderInstaller : CommonDatabaseConnectionStringProvidersInstallerBase
    {
        private readonly string odsConnectionString;

        public DatabaseConnectionStringProviderInstaller(string odsConnectionString)
        {
            this.odsConnectionString = odsConnectionString;
        }

        protected override void RegisterOdsDatabaseConnectionStringProvider(IWindsorContainer container)
        {
            // Provide a literal connection string for the ODS
            var connectionStringProvider = new LiteralDatabaseConnectionStringProvider(odsConnectionString);

            container.Register(Component
                .For<IDatabaseConnectionStringProvider>()
                .NamedForDatabase(Databases.Ods)
                .Instance(connectionStringProvider));
        }
    }
}
