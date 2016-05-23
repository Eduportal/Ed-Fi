using Castle.Windsor;
using EdFi.Ods.Api._Installers;
using Microsoft.Owin;

[assembly: OwinStartup("SharedInstance", typeof(EdFi.Ods.Api.Startup.SharedInstanceStartup))]
namespace EdFi.Ods.Api.Startup
{
    public class SharedInstanceStartup: StartupBase
    {
        protected override void InstallConfigurationSpecificInstaller(IWindsorContainer container)
        {
            container.Install(new SharedInstanceInstaller());
        }
    }
}
