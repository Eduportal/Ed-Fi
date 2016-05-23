using Castle.Windsor;
using EdFi.Ods.Common._Installers;

namespace EdFi.Ods.Api._Installers
{
    /// <summary>
    /// Provides container registrations for the sandbox environment.
    /// </summary>
    public class SandboxInstaller : ConfigurationSpecificInstallerBase
    {
        protected override void RegisterDatabaseConnectionStringProviders(IWindsorContainer container)
        {
            container.Install(new SandboxEnvironmentDatabaseConnectionStringsInstaller());
        }
    }
}