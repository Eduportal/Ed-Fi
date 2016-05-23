using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Caching;
using EdFi.Common.Configuration;
using EdFi.Common.Context;
using EdFi.Common.Database;
using EdFi.Ods.Common._Installers.ComponentNaming;
using EdFi.Ods.Common.Database;

namespace EdFi.Ods.WebService.Tests.Owin
{
    internal abstract class OwinTestStartupBase : EdFi.Ods.Api.Startup.StartupBase
    {
        protected abstract void InstallTestSpecificInstaller(IWindsorContainer container);

        protected override void InstallConfigurationSpecificInstaller(IWindsorContainer container)
        {
            container.Register(Component.For<IConfigConnectionStringsProvider>().ImplementedBy<AppConfigConnectionStringsProvider>());
            container.Register(Component.For<IConfigValueProvider>().ImplementedBy<AppConfigValueProvider>());
            container.Register(Component.For<IConfigSectionProvider>().ImplementedBy<AppConfigSectionProvider>());
            container.Register(Component.For<ICacheProvider>().ImplementedBy<AspNetCacheProvider>());

            container.Register(Component.For<IDatabaseConnectionStringProvider>()
                                        .Named("IDatabaseConnectionStringProvider.Admin")
                                        .ImplementedBy<NamedDatabaseConnectionStringProvider>()
                                        .DependsOn(Dependency.OnValue("connectionStringName", "EdFi_Admin")));

            container.Register(Component.For<IDatabaseConnectionStringProvider>()
                                        .Named("IDatabaseConnectionStringProvider.BulkOperations")
                                        .ImplementedBy<NamedDatabaseConnectionStringProvider>()
                                        .DependsOn(Dependency.OnValue("connectionStringName", "BulkOperationDbContext")));

            container.Register(Component.For<IDatabaseConnectionStringProvider>()
                                        .Named("IDatabaseConnectionStringProvider.EduId")
                                        .ImplementedBy<NamedDatabaseConnectionStringProvider>()
                                        .DependsOn(Dependency.OnValue("connectionStringName", "EduIdContext")));

            container.Register(Component.For<IDatabaseConnectionStringProvider>()
                                        .Named("IDatabaseConnectionStringProvider.Master")
                                        .ImplementedBy<NamedDatabaseConnectionStringProvider>()
                                        .DependsOn(Dependency.OnValue("connectionStringName", "EdFi_master")));

            container.Register(Component.For<IContextStorage>().ImplementedBy<HashtableContextStorage>().IsDefault());

            RegisterOdsDatabase(container);

            InstallTestSpecificInstaller(container);
        }

        protected virtual void RegisterOdsDatabase(IWindsorContainer container)
        {
            container.Register(Component.For<IDatabaseConnectionStringProvider>()
                                        .Named("IDatabaseConnectionStringProvider.Ods")
                                        .ImplementedBy<PrototypeWithDatabaseNameOverrideDatabaseConnectionStringProvider>()
                                        .DependsOn(Dependency.OnValue("prototypeConnectionStringName", "EdFi_Ods"))
                                        .DependsOn(Dependency.OnComponent(typeof(IDatabaseNameProvider), DatabaseNameStrategyRegistrationKeys.Sandbox)));
        }
    }    
}
