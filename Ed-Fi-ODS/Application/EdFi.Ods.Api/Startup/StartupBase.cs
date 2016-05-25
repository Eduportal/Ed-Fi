using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Dispatcher;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Database;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Api.Architecture;
using EdFi.Ods.Api.Common;
using EdFi.Ods.Api._Installers;
using EdFi.Ods.Api.Common.Authorization;
using EdFi.Ods.Api.Common.Filters;
using EdFi.Ods.Api.Services;
using EdFi.Ods.Api.Services.ActionFilters;
using EdFi.Ods.Api.Services.Filters;
using EdFi.Ods.Common._Installers.ComponentNaming;
using EdFi.Ods.Common.Context;
using EdFi.Ods.Common.Security;
using EdFi.Ods.Common.Utils.Extensions;
using EdFi.Ods.Entities.Repositories.NHibernate.Architecture;
using EdFi.Ods.Entities.Repositories.NHibernate.Architecture.Bytecode;
using EdFi.Ods.Security.Metadata.Repositories;
using EdFi.Ods.Security.Profiles;
using log4net;
using log4net.Config;
using Microsoft.Owin.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NHibernate;
using NHibernate.Cfg;
using Owin;
using Environment = NHibernate.Cfg.Environment;
using IDependencyResolver = System.Web.Http.Dependencies.IDependencyResolver;

namespace EdFi.Ods.Api.Startup
{
    public abstract class StartupBase : IDisposable
    {
        protected readonly IWindsorContainer Container = new WindsorContainerEx();

        public virtual void Configuration(IAppBuilder appBuilder)
        {
            InitializeContainer(Container);
            Container.Install(new WebApiInstaller());
            InstallConfigurationSpecificInstaller(Container);

            // NHibernate initialization
            (new NHibernateConfigurator()).Configure(Container);

            var httpConfig = new HttpConfiguration
            {
                DependencyResolver = Container.Resolve<IDependencyResolver>()
            };

            // Replace the default controller selector with one based on the final namespace segment (to enable support of Profiles)
            httpConfig.Services.Replace(
                typeof(IHttpControllerSelector), 
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

            // Populate the Profiles in the Admin Database if the IAdminProfileNamesPublisher has been registered
            // since it is registered in the security components installer and that may not always be registered.
            if (Container.Kernel.HasComponent(typeof(IAdminProfileNamesPublisher)))
            {
                var adminProfileNamesPublisher = Container.Resolve<IAdminProfileNamesPublisher>();
                adminProfileNamesPublisher.PublishProfilesAsync();
            }
        }

        protected virtual void ConfigureDelegatingHandlers(HttpConfiguration httpConfig, IEnumerable<DelegatingHandler> handlers)
        {
            if (handlers == null) return;
            foreach (var delegatingHandler in handlers)
            {
                httpConfig.MessageHandlers.Add(delegatingHandler);
            }
        }

        protected virtual void InitializeContainer(IWindsorContainer container)
        {
            container.AddFacility<TypedFactoryFacility>();
            container.AddFacility<DatabaseConnectionStringProviderFacility>();
            container.AddSupportForEmptyCollections();

            // Web API Dependency Injection
            container.Register(
                Component.For<IDependencyResolver>().Instance(new WindsorDependencyResolver(Container))
                );
        }

        protected abstract void InstallConfigurationSpecificInstaller(IWindsorContainer container);

        protected virtual void ConfigureFormatters(HttpConfiguration config)
        {
            // Replace existing JSON formatter to be profiles-aware
            var existingJsonFormatter = config.Formatters.SingleOrDefault(f => f is JsonMediaTypeFormatter);

            // Remove the original one
            if (existingJsonFormatter != null)
                config.Formatters.Remove(existingJsonFormatter);

            // Add our customized one, supporting dynamic addition of media types for deserializing messages
            config.Formatters.Insert(0, new ProfilesContentTypeAwareJsonMediaTypeFormatter());

            // Make Json the default response type for api requests
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            // configure JSON serialization
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            config.Formatters.JsonFormatter.RequiredMemberSelector = new NoRequiredMemberSelector();
        }

        protected virtual void RegisterFilters(HttpConfiguration config)
        {
            RegisterCoreFilters(config);
            RegisterAuthenticationFilters(config);
            RegisterAuthorizationFilters(config);
        }

        protected void RegisterCoreFilters(HttpConfiguration config)
        {
            var showErrors = (config.IncludeErrorDetailPolicy != IncludeErrorDetailPolicy.Never);
            config.Filters.Add(new ExceptionHandlingFilter(showErrors));
            config.Filters.Add(new RouteValuesContextFilter(Container.Resolve<ISchoolYearContextProvider>()));
        }

        protected void RegisterAuthorizationFilters(HttpConfiguration config)
        {
            config.Filters.Add(
                new EdFiAuthorizationFilter(
                    Container.Resolve<IEdFiAuthorizationProvider>(),
                    Container.Resolve<ISecurityRepository>()));
            config.Filters.Add(
                new ProfilesAuthorizationFilter(Container.Resolve<IApiKeyContextProvider>(), Container.Resolve<IProfileResourcesProvider>()));
        }

        protected void RegisterAuthenticationFilters(HttpConfiguration config)
        {
            config.Filters.Add(
                new OAuthAuthenticationFilter(
                    Container.Resolve<IOAuthTokenValidator>(),
                    Container.Resolve<IApiKeyContextProvider>(),
                    Container.Resolve<IClaimsIdentityProvider>()));
        }

        protected virtual void ConfigureRoutes(HttpConfiguration config)
        {
            //Map all attribute routes (Web 2.0 feature)
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "OAuthAuthorize",
                routeTemplate: "oauth/authorize",
                defaults: new { controller = "Authorize" }
            );

            config.Routes.MapHttpRoute(
                name: "OAuthToken",
                routeTemplate: "oauth/token",
                defaults: new { controller = "Token" }
            );

            //This route and target swagger controller only acts as a design-time code generator for meta data contained for the other section. 
            //Generated json is stored in static files and served from the other Metadatacontroller. This is to allow customization of the json
            //beyond what can get auto-generated
            config.Routes.MapHttpRoute(
                name: "Swagger",
                routeTemplate: "swagger/other/api-docs/{id}",
                defaults: new
                {
                    controller = "swagger",
                    id = RouteParameter.Optional
                }
            );

            config.Routes.MapHttpRoute(
                name: "MetadataSections",
                routeTemplate: "metadata/",
                defaults: new
                {
                    controller = "metadata",
                }
            );

            config.Routes.MapHttpRoute(
                name: "Metadata",
                routeTemplate: "metadata/{section}/api-docs/{id}",
                defaults: new
                {
                    controller = "metadata",
                    id = RouteParameter.Optional
                }
            );

            config.Routes.MapHttpRoute(
                name: "OtherSectionWithoutSchoolYear",
                routeTemplate: "api/v{apiVersion}/{controller}/{id}",
                defaults: new
                {
                    apiVersion = "2.0",
                    id = RouteParameter.Optional,
                },
                constraints: new
                {
                    controller = @"^(?i)(identities|schoolidentities|staffidentities)$",
                }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/v{apiVersion}/{schoolYearFromRoute}/{controller}/{id}",
                defaults: new
                {
                    apiVersion = "2.0",
                    id = RouteParameter.Optional,
                },
                constraints: new
                {
                    //do not use this path for the swagger, and other controllers not needing school year
                    controller = @"^((?!(swagger|identities|schoolidentities|staffidentities)).)*$",
                    schoolYearFromRoute = @"^\d{4}$"
                });
        }

        public void Dispose()
        {
            Container.Dispose();
        }
    }
}
