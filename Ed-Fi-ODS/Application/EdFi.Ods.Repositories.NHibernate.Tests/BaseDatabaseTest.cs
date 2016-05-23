using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Caching;
using EdFi.Common.Configuration;
using EdFi.Common.Context;
using EdFi.Common.Database;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Api.Common;
using EdFi.Ods.Common.Database;
using EdFi.Ods.Common._Installers;
using EdFi.Ods.Common._Installers.ComponentNaming;
using EdFi.Ods.Entities.Common.Caching;
using EdFi.Ods.Entities.Common._Installers;
using EdFi.Ods.Entities.NHibernate;
using EdFi.Ods.Entities.Repositories.NHibernate.Architecture;
using EdFi.Ods.Entities.Repositories.NHibernate.Architecture.Bytecode;
using EdFi.Ods.Entities.Repositories.NHibernate._Installers;
using EdFi.Ods.Security.Claims;
using EdFi.Ods.Security.Metadata.Repositories;
using EdFi.Ods.Tests.EdFi.Ods.Security._Stubs;
using NHibernate;
using NHibernate.Cfg;
using NUnit.Framework;
using Rhino.Mocks;
using Test.Common;
using Environment = NHibernate.Cfg.Environment;

namespace EdFi.Ods.Repositories.NHibernate.Tests
{
    public abstract class BaseDatabaseTest
    {
        protected abstract string BaseDatabase { get; }
        protected abstract string DatabaseName { get; }
        protected IWindsorContainer Container { get; set; }
        protected ISessionFactory SessionFactory { get; set; }

        [SetUp]
        public void BaseSetUp()
        {
            var databaseHelper = new DatabaseHelper();
            databaseHelper.CopyDatabase(BaseDatabase, DatabaseName);
            RegisterDependencies();
            SessionFactory = Container.Resolve<ISessionFactory>();
        }

        [TearDown]
        public void BaseTearDown()
        {
            var databaseHelper = new DatabaseHelper();
            databaseHelper.DropDatabase(DatabaseName);
        }

        private void RegisterDependencies()
        {
            var factory = new InversionOfControlContainerFactory();
            Container = factory.CreateContainer(c =>
            {
                c.AddFacility<TypedFactoryFacility>();
                c.AddFacility<DatabaseConnectionStringProviderFacility>();
            });
            Container.Install(new EdFiOdsCommonInstaller());
            Container.Install(new EdFiOdsRepositoriesNHibernateInstaller());
            Container.Install(new EdFiOdsEntitiesCommonInstaller());
            Container.Register(Component.For<IConfigConnectionStringsProvider>().ImplementedBy<AppConfigConnectionStringsProvider>());
            Container.Register(Component.For<IConfigValueProvider>().ImplementedBy<AppConfigValueProvider>());
            Container.Register(Component.For<IConfigSectionProvider>().ImplementedBy<AppConfigSectionProvider>());

            var databaseNameProvider = MockRepository.GenerateStub<IDatabaseNameProvider>();
            databaseNameProvider.Stub(d => d.GetDatabaseName()).Return(DatabaseName);

            Container.Register(Component.For<IDatabaseNameProvider>().Instance(databaseNameProvider).IsDefault().Named(DatabaseNameStrategyRegistrationKeys.Sandbox));

            Container.Register(Component.For<IDatabaseConnectionStringProvider>()
                            .Named("IDatabaseConnectionStringProvider.Ods")
                            .ImplementedBy<PrototypeWithDatabaseNameOverrideDatabaseConnectionStringProvider>()
                            .DependsOn(Dependency.OnValue("prototypeConnectionStringName", "EdFi_Ods"))
                            .DependsOn(Dependency.OnComponent(typeof(IDatabaseNameProvider), DatabaseNameStrategyRegistrationKeys.Sandbox)));

            Container.Register(Component.For<ICacheProvider>().ImplementedBy<MemoryCacheProvider>());
            InitializeNHibernate(Container);

            // Register security component
            Container.Register(Component
                .For<IAuthorizationContextProvider>()
                .ImplementedBy<AuthorizationContextProvider>());

            Container.Register(Component
                .For<IContextStorage>()
                .ImplementedBy<HashtableContextStorage>());

            Container.Register(
                Component.For<IClaimsIdentityProvider>()
                    .ImplementedBy<ClaimsIdentityProvider>()
                    .LifestyleTransient());

            Container.Register(
                Component.For<ISecurityRepository>()
                .ImplementedBy<StubSecurityRepository>());
        }

        private static void InitializeNHibernate(IWindsorContainer container)
        {
            Environment.BytecodeProvider = new BytecodeProvider(container);
            var nHibernateConfiguration = new Configuration().Configure();

            nHibernateConfiguration.AddCreateDateHooks();

            container
                .Register(Component
                    .For<ISessionFactory>()
                    .UsingFactoryMethod(nHibernateConfiguration.BuildSessionFactory)
                    .LifeStyle.Singleton);

            container.Register(Component
                .For<EdFiOdsConnectionProvider>()
                .DependsOn(Dependency
                    .OnComponent(
                        typeof(IDatabaseConnectionStringProvider),
                        typeof(IDatabaseConnectionStringProvider).GetServiceNameWithSuffix(Databases.Ods.ToString()))));
        }

    }
}
