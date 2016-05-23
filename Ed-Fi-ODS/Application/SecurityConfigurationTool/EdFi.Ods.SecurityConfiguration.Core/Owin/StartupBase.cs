using System.Web.Http;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.SecurityConfiguration.Services.Implementation;
using log4net.Config;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using OWIN.Windsor.DependencyResolverScopeMiddleware;

namespace EdFi.Ods.SecurityConfiguration.Core.Owin
{
    public abstract class StartupBase
    {
        public virtual void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            RegisterApiRoutes(config);

            var container = BootstrapContainer(config);

            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = false,
                FileSystem = new PhysicalFileSystem("."),
                EnableDefaultFiles = true
            };

            app
                .UseWindsorDependencyResolverScope(config, container)
                .UseWebApi(config)
                .UseFileServer(options)
                .UseStaticFiles(new StaticFileOptions());

            ConfigureSerialization(config);

            XmlConfigurator.Configure();
        }

        protected abstract void RegisterApiRoutes(HttpConfiguration config);

        protected abstract void RegisterControllers(IWindsorContainer container);

        protected virtual void RegisterIocResolvers(IWindsorContainer container)
        {
            container.AddFacility<TypedFactoryFacility>();

            container.Register(

                Component.For<IUsersContext>().ImplementedBy<UsersContext>().LifestyleTransient(),
                Component.For<IAdminContextFactory>().AsFactory(),

                Classes.FromAssemblyContaining<IAssemblyLocator>()
                    .Where(x => x.Name.EndsWith("Provider"))
                    .WithService.AllInterfaces(),

                Classes.FromAssemblyContaining<IAssemblyLocator>()
                    .Where(x => x.Name.EndsWith("Service"))
                    .WithService.AllInterfaces()
                );

            RegisterControllers(container);
        }

        protected virtual IWindsorContainer BootstrapContainer(HttpConfiguration config)
        {
            var container = new WindsorContainer(); 
            RegisterControllers(container);
            RegisterIocResolvers(container);

            return container;
        }

        private static void ConfigureSerialization(HttpConfiguration config)
        {
            var formatters = config.Formatters;
            var jsonFormatter = formatters.JsonFormatter;
            var settings = jsonFormatter.SerializerSettings;
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

    }
}