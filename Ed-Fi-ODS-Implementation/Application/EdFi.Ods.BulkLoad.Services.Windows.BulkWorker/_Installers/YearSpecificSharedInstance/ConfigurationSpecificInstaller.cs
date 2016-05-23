using Castle.Windsor;
using EdFi.Ods.Common._Installers;

namespace EdFi.Ods.BulkLoad.Services.Windows.BulkWorker._Installers.YearSpecificSharedInstance
{
    public class ConfigurationSpecificInstaller : ConfigurationSpecificInstallerBase
    {
        protected override void RegisterDatabaseConnectionStringProviders(IWindsorContainer container)
        {
            container.Install(new YearSpecificSharedInstanceEnvironmentDatabaseConnectionStringInstaller());
        }
    }
}