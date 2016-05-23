using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Dependencies;
using System.Web.Http.Dispatcher;
using Castle.MicroKernel.Registration;
using EdFi.Ods.Api.Architecture;
using EdFi.Ods.Api.Common;
using EdFi.Ods.Api.Models.TestProfiles;
using EdFi.Ods.Api.Startup;
using EdFi.Ods.Common.Utils.Profiles;
using log4net.Config;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Owin;

namespace EdFi.Ods.WebService.Tests.Profiles
{
    public class ProfilesSdkTestStartup : ProfilesTestStartup
    {
        public override void Configuration(IAppBuilder appBuilder)
        {
            InitializeContainer(Container);

            // TODO: GKM - Profiles - Modified installer in use for testing purposes
            Container.Install(new ProfilesTestingWebApiInstaller());

            //Call custom owin component
            appBuilder.Use(typeof (ProfilesSdkCustomMiddleware));

            // TODO: GKM - Profiles - Need to register additional controllers for the test profiles
            // Controllers
            Container.Register(
                Classes.FromAssemblyContaining<Marker_EdFi_Ods_Api_Models_TestProfiles>()
                    .BasedOn<ApiController>()
                    .LifestyleTransient());

            InstallConfigurationSpecificInstaller(Container);

            // TODO: GKM - No NHibernate desired for testing
            // NHibernate initialization
            //(new NHibernateConfigurator()).Configure(Container);

            var httpConfig = new HttpConfiguration
            {
                DependencyResolver = Container.Resolve<IDependencyResolver>()
            };

            // Replace the default controller selector with one based on the final namespace segment (to enable support of Profiles)
            httpConfig.Services.Replace(
                typeof (IHttpControllerSelector),
                new ProfilesAwareHttpControllerSelector(httpConfig, Container.Resolve<IProfileResourcesProvider>()));

            httpConfig.EnableSystemDiagnosticsTracing();
            httpConfig.EnableCors(new EnableCorsAttribute("*", "*", "*", "*"));
            ConfigureRoutes(httpConfig);
            ConfigureFormatters(httpConfig);
            ConfigureDelegatingHandlers(httpConfig, Container.ResolveAll<DelegatingHandler>());
            RegisterFilters(httpConfig);

            appBuilder.UseWebApi(httpConfig);

            XmlConfigurator.Configure();
            appBuilder.SetLoggerFactory(new Log4NetLoggerFactory());
        }

        private class ProfilesSdkCustomMiddleware : OwinMiddleware
        {
            private OwinMiddleware _next;

            public ProfilesSdkCustomMiddleware(OwinMiddleware next) : base(next)
            {
                _next = next;
            }

            public async override Task Invoke(IOwinContext context)
            {
                var request = context.Request;
                var response = context.Response;
                
                var requestAcceptExists = (request.Accept != null);
                
                var isApiResourceRequest = (requestAcceptExists
                                            && request.Accept.IsEdFiContentType()
                                            && request.Method == "GET");
                
                var originalRequestedContentType = string.Empty;
                if (isApiResourceRequest)
                {
                    originalRequestedContentType = request.Accept;
                    request.Accept = "application/json";
                }
                
                response.OnSendingHeaders(state =>
                {
                    var owinResponseState = (OwinResponse) state;
                    if (isApiResourceRequest
                        && owinResponseState.StatusCode == 200)
                    {
                        owinResponseState.Headers.Set("Content-Type",originalRequestedContentType);
                    }
                    
                }, response);

                await _next.Invoke(context);
            }
        }
    }
}