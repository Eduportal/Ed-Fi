using Castle.Windsor;
using EdFi.Ods.Common._Installers;

namespace EdFi.Workers.BulkLoad._Installers.Sandbox
{
    /// <summary>
    /// Provides container registrations for the sandbox environment.
    /// </summary>
    public class ConfigurationSpecificInstaller : ConfigurationSpecificInstallerBase
    {
        protected override void RegisterDatabaseConnectionStringProviders(IWindsorContainer container)
        {
            container.Install(new SandboxEnvironmentDatabaseConnectionStringsInstaller());
        }
    }
}