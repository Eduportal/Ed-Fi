using System.Web.Http;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Ods.SecurityConfiguration.Core.Owin;
using log4net;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup("SecurityConfigurationAdministrationToolStartup", typeof(AdministrationToolCoreStartup))]
namespace EdFi.Ods.SecurityConfiguration.Core.Owin
{
    public class AdministrationToolCoreStartup : StartupBase
    {
        public override void Configuration(IAppBuilder app)
        {
            base.Configuration(app);
            LogManager.GetLogger(typeof(AdministrationToolCoreStartup)).Info("Application started.");
        }

        protected override void RegisterApiRoutes(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
        }

        protected override void RegisterControllers(IWindsorContainer container)
        {
            container.Register(
                Classes.FromThisAssembly()
                    .InNamespace("EdFi.Ods.SecurityConfiguration.Core.Controllers.Administration")
                    .LifestyleScoped(),
                Classes.FromThisAssembly()
                    .InNamespace("EdFi.Ods.SecurityConfiguration.Core.Controllers.Shared")
                    .LifestyleScoped()
                );
        }
    }
}
