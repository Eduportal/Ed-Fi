using Castle.Windsor;
using EdFi.Ods.Common._Installers;

namespace EdFi.Ods.Api._Installers
{
    /// <summary>
    /// Provides container registrations for the shared instance environment.
    /// </summary>
    public class SharedInstanceInstaller : ConfigurationSpecificInstallerBase
    {
        protected override void RegisterDatabaseConnectionStringProviders(IWindsorContainer container)
        {
            container.Install(new SharedInstanceEnvironmentDatabaseConnectionStringInstaller());
        }
    }
}