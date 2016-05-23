using Castle.Windsor;
using EdFi.Ods.Api._Installers;
using Microsoft.Owin;

[assembly: OwinStartup("YearSpecific", typeof(EdFi.Ods.Api.Startup.YearSpecificStartup))]
namespace EdFi.Ods.Api.Startup
{
    public class YearSpecificStartup : StartupBase
    {
        protected override void InstallConfigurationSpecificInstaller(IWindsorContainer container)
        {
            container.Install(new YearSpecificInstaller());
        }
    }
}
