using Castle.Windsor;
using EdFi.Ods.Common._Installers;

namespace EdFi.Ods.Api._Installers
{
    /// <summary>
    /// Provides container registrations for the shared instance environment with year-specific ODS databases.
    /// </summary>
    public class YearSpecificInstaller : ConfigurationSpecificInstallerBase
    {
        protected override void RegisterDatabaseConnectionStringProviders(IWindsorContainer container)
        {
            container.Install(new YearSpecificSharedInstanceEnvironmentDatabaseConnectionStringInstaller());
        }
    }
}