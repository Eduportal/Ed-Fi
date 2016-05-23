using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Dependencies;
using System.Web.Http.Dispatcher;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Caching;
using EdFi.Common.Configuration;
using EdFi.Common.Context;
using EdFi.Ods.Api.Architecture;
using EdFi.Ods.Api.Common;
using EdFi.Ods.Api.Common.Filters;
using EdFi.Ods.Api.Models.TestProfiles;
using EdFi.Ods.Api.Startup;
using EdFi.Ods.Api._Installers;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Common.Security;
using EdFi.Ods.Entities.Common.Caching;
using EdFi.Ods.Entities.NHibernate;
using EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers;
using EdFi.TestObjects;
using EdFi.TestObjects.Builders;
using log4net.Config;
using Microsoft.Owin.Logging;
using Owin;
using Rhino.Mocks;
using TechTalk.SpecFlow;

namespace EdFi.Ods.WebService.Tests.Profiles
{
    public class ProfilesTestStartup : StartupBase
    {
        // Provides empty overrides to disable certain components for testing purposes.
        internal class ProfilesTestingWebApiInstaller : WebApiInstaller
        {
            protected override void RegisterNHibernateComponents(IWindsorContainer container)
            {
            }

            protected override void RegisterSecurityComponents(IWindsorContainer container)
            {
            }
        }

        public IWindsorContainer InternalContainer
        {
            get { return base.Container; }
        }

        public override void Configuration(IAppBuilder appBuilder)
        {
            InitializeContainer(Container);

            // TODO: GKM - Profiles - Modified installer in use for testing purposes
            Container.Install(new ProfilesTestingWebApiInstaller());

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
        }

        protected override void InstallConfigurationSpecificInstaller(IWindsorContainer container)
        {
            container.Register(
                Component.For<IConfigConnectionStringsProvider>()
                         .ImplementedBy<AppConfigConnectionStringsProvider>());
            container.Register(
                Component.For<IConfigValueProvider>().ImplementedBy<AppConfigValueProvider>());
            container.Register(
                Component.For<IConfigSectionProvider>().ImplementedBy<AppConfigSectionProvider>());
            container.Register(Component.For<ICacheProvider>().ImplementedBy<AspNetCacheProvider>());

            //mock ApikeyContextProvider for the ProfilesAuthorizationFilter Tests
            var suppliedApiKeyContextProvider = MockRepository.GenerateStub<IApiKeyContextProvider>();

            //suppliedApiKeyContextProvider.Expect(x => x.GetApiKeyContext())
            //    .Do((Func<ApiKeyContext>)(delegate { return new ApiKeyContext(null, null, null, null, ScenarioContext.Current.Get<List<string>>("assignedProfiles")); }));

            suppliedApiKeyContextProvider.Expect(x => x.GetApiKeyContext()).Do((Func<ApiKeyContext>)(GetSuppliedApiKeyContext));

            container.Register(Component.For<IApiKeyContextProvider>().Instance(suppliedApiKeyContextProvider).IsDefault());

            //container.Register(Component.For<IDatabaseConnectionStringProvider>()
            //                            .Named("IDatabaseConnectionStringProvider.Admin")
            //                            .ImplementedBy<NamedDatabaseConnectionStringProvider>()
            //                            .DependsOn(Dependency.OnValue("connectionStringName", "EdFi_Admin")));

            //container.Register(Component.For<IDatabaseConnectionStringProvider>()
            //                            .Named("IDatabaseConnectionStringProvider.BulkOperations")
            //                            .ImplementedBy<NamedDatabaseConnectionStringProvider>()
            //                            .DependsOn(Dependency.OnValue("connectionStringName", "BulkOperationDbContext")));

            //container.Register(Component.For<IDatabaseConnectionStringProvider>()
            //                            .Named("IDatabaseConnectionStringProvider.EduId")
            //                            .ImplementedBy<NamedDatabaseConnectionStringProvider>()
            //                            .DependsOn(Dependency.OnValue("connectionStringName", "EduIdContext")));

            //container.Register(Component.For<IDatabaseConnectionStringProvider>()
            //                            .Named("IDatabaseConnectionStringProvider.Master")
            //                            .ImplementedBy<NamedDatabaseConnectionStringProvider>()
            //                            .DependsOn(Dependency.OnValue("connectionStringName", "EdFi_master")));

            container.Register(
                Component.For<IContextStorage>().ImplementedBy<HashtableContextStorage>().IsDefault());

            //RegisterOdsDatabase(container);

            container.Register(
                Component
                    .For<ITestObjectFactory>()
                    .Instance(CreateTestObjectFactory()));

            RegisterFakeRepository(container);

            //InstallTestSpecificInstaller(container);

            TypesAndDescriptorsCache.GetCache = () => new FakeTypesAndDescriptorsCache();
            PersonUniqueIdToUsiCache.GetCache = () => new FakePersonUniqueIdToUsiCache();
        }

        protected virtual void RegisterFakeRepository(IWindsorContainer container)
        {
            container.Register(
                Component.For(typeof (IUpsertEntity<>))
                    .Forward(typeof (IGetEntityById<>))
                    .Forward(typeof (IGetEntitiesBySpecification<>))
                    .ImplementedBy(typeof (FakeRepository<>)));
        }

        private static TestObjectFactory CreateTestObjectFactory()
        {
            var builderFactory = new BuilderFactory();

            builderFactory.AddToFront<EntityParentBackreferenceValueBuilder>();
            builderFactory.AddToFront<FakeEdFiTypeValueBuilder>();
            builderFactory.AddToFront<FakeEdFiDescriptorValueBuilder>();
            builderFactory.AddToFront<FakeStudentIdentifierValueBuilder>();
            StringValueBuilder.GenerateEmptyStrings = false;
            StringValueBuilder.GenerateNulls = false;

            var testObjectFactory = new TestObjectFactory(
                builderFactory.GetBuilders(),
                new SystemActivator(),
                new CustomAttributeProvider());
            return testObjectFactory;
        }

        protected override void RegisterFilters(HttpConfiguration config)
        {
            RegisterCoreFilters(config);
            config.Filters.Add(new ProfilesAuthorizationFilter(Container.Resolve<IApiKeyContextProvider>(), Container.Resolve<IProfileResourcesProvider>()));

            // Security concerns - should not be part of "core"
            //config.Filters.Add(new OAuthAuthenticationFilter(Container.Resolve<IOAuthTokenValidator>(), Container.Resolve<IApiKeyContextProvider>(), Container.Resolve<IClaimsIdentityProvider>()));
            //config.Filters.Add(new EdFiAuthorizationFilter(Container.Resolve<IEdFiAuthorizationProvider>(), Container.Resolve<ISecurityRepository>()));
        }

        private ApiKeyContext GetSuppliedApiKeyContext()
        {
            List<string> profiles;
            
            if (ScenarioContext.Current == null || !ScenarioContext.Current.TryGetValue(ScenarioContextKeys.AssignedProfiles, out profiles))
                profiles = new List<string>();

            return new ApiKeyContext(null, null, null, null, profiles);
        }
    }
}