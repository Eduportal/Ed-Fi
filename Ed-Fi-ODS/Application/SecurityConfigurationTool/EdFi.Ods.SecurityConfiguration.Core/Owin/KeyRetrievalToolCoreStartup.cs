using System.Web.Http;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Ods.SecurityConfiguration.Core.Owin;
using log4net;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup("SecurityConfigurationKeyRetrievalToolCoreStartup", typeof(KeyRetrievalToolCoreStartup))]
namespace EdFi.Ods.SecurityConfiguration.Core.Owin
{
    public class KeyRetrievalToolCoreStartup : StartupBase
    {
        public override void Configuration(IAppBuilder app)
        {
            base.Configuration(app);
            LogManager.GetLogger(typeof(KeyRetrievalToolCoreStartup)).Info("Application started.");
        }

        protected override void RegisterApiRoutes(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
        }

        protected override void RegisterControllers(IWindsorContainer container)
        {
            container.Register(
                Classes.FromThisAssembly()
                    .InNamespace("EdFi.Ods.SecurityConfiguration.Core.Controllers.KeyRetrieval")
                    .LifestyleScoped(),
                Classes.FromThisAssembly()
                    .InNamespace("EdFi.Ods.SecurityConfiguration.Core.Controllers.Shared")
                    .LifestyleScoped()
                );
        }
    }
}