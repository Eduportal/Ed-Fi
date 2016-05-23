using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Castle.Windsor;
using Castle.Windsor.Installer;
using EdFi.Common.InversionOfControl;
using EdFi.Ods.Admin.Filters;
using EdFi.Ods.Admin.Infrastructure;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.Admin.Services;
using EdFi.Ods.Admin.Web.App_Start;
using WebMatrix.WebData;
using log4net.Config;

namespace EdFi.Ods.Admin.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        private static IWindsorContainer container;

        private static void BootstrapContainer()
        {
            container = new WindsorContainerEx().Install(FromAssembly.This());

            // MVC Dependency Resolution 
            var controllerFactory = new CastleControllerFactory(container.Kernel);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            // WebApi Dependency Resolution
            var dependencyResolver = new CastleDependencyResolver(container);
            GlobalConfiguration.Configuration.DependencyResolver = dependencyResolver;
        }

        protected void Application_Start()
        {
            XmlConfigurator.Configure();
            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AuthConfig.RegisterAuth();

            GlobalFilters.Filters.Add(new SetCurrentUserInfoAttribute(() => container.Resolve<ISecurityService>()));
            InitializeWebSecurity();

            //Do this last to make sure we aren't trying to use the container during Application_Start.  We can't use the container during Application_Start
            //because we are registering some dependencies with a PerWebRequest lifecycle.  Castle cannot resolve those dependencies outside of the request
            //lifecycle.
            BootstrapContainer();
        }

        private static void InitializeWebSecurity()
        {
            WebSecurity.InitializeDatabaseConnection("EdFi_Admin", UsersContext.UserTableName, UsersContext.UserIdColumn, UsersContext.UserNameColumn, autoCreateTables: true);
        }
    }
}