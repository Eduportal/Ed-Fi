using Castle.Windsor;
using EdFi.Ods.Common._Installers;

namespace EdFi.Workers.BulkLoad._Installers.SharedInstance
{
    /// <summary>
    /// Provides container registrations for the shared instance environment.
    /// </summary>
    public class ConfigurationSpecificInstaller : ConfigurationSpecificInstallerBase
    {
        protected override void RegisterDatabaseConnectionStringProviders(IWindsorContainer container)
        {
            container.Install(new SharedInstanceEnvironmentDatabaseConnectionStringInstaller());
        }
    }
}