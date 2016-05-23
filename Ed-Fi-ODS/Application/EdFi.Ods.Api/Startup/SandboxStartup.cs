using Castle.Windsor;
using EdFi.Ods.Api._Installers;
using Microsoft.Owin;

[assembly: OwinStartup("Sandbox", typeof(EdFi.Ods.Api.Startup.SandboxStartup))]
namespace EdFi.Ods.Api.Startup
{
    public class SandboxStartup: StartupBase
    {
        protected override void InstallConfigurationSpecificInstaller(IWindsorContainer container)
        {
            container.Install(new SandboxInstaller());
        }
    }
}
