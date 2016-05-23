using Castle.Windsor;
using EdFi.Ods.Common._Installers;

namespace EdFi.Workers.BulkLoad._Installers.YearSpecificSharedInstance
{
    /// <summary>
    /// Provides container registrations for the shared instance environment with year-specific ODS databases.
    /// </summary>
    public class ConfigurationSpecificInstaller : ConfigurationSpecificInstallerBase
    {
        protected override void RegisterDatabaseConnectionStringProviders(IWindsorContainer container)
        {
            container.Install(new YearSpecificSharedInstanceEnvironmentDatabaseConnectionStringInstaller());
        }
    }
}