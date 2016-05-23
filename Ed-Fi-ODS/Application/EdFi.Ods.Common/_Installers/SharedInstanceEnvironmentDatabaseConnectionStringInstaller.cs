using Castle.Windsor;
using EdFi.Ods.Common._Installers.ComponentNaming;

namespace EdFi.Ods.Common._Installers
{
    /// <summary>
    /// Installs database connection providers that are handled differently for the 
    /// shared instance environment.
    /// </summary>
    public class SharedInstanceEnvironmentDatabaseConnectionStringInstaller 
        : CommonDatabaseConnectionStringProvidersInstallerBase
    {
        protected override void RegisterOdsDatabaseConnectionStringProvider(IWindsorContainer container)
        {
            // Single instance ODS version
            CommonDatabaseConnectionStringProvidersInstallerBase.RegisterNamedConnectionStringProvider(container, Databases.Ods, Databases.Ods.GetConnectionStringName());
        }
    }
}