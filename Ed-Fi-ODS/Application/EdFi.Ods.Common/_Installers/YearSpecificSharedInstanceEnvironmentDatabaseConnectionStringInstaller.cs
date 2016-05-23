using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Database;
using EdFi.Ods.Common.Database;
using EdFi.Ods.Common._Installers.ComponentNaming;

namespace EdFi.Ods.Common._Installers
{
    public class YearSpecificSharedInstanceEnvironmentDatabaseConnectionStringInstaller
        : CommonDatabaseConnectionStringProvidersInstallerBase
    {
        protected override void RegisterOdsDatabaseConnectionStringProvider(IWindsorContainer container)
        {
            container.Register(Component
                .For<IDatabaseConnectionStringProvider>()
                .NamedForDatabase(Databases.Ods)
                .ImplementedBy<PrototypeWithDatabaseNameOverrideDatabaseConnectionStringProvider>()
                .DependsOn(Dependency.OnValue("prototypeConnectionStringName", Databases.Ods.GetConnectionStringName()))
                .DependsOn(Dependency
                    .OnComponent(
                        typeof(IDatabaseNameProvider),
                        DatabaseNameStrategyRegistrationKeys.YearSpecificOds))
                );
        }

        protected virtual void RegisterIDatabaseNameProvider(IWindsorContainer container)
        {
            container.Register(Component
                .For<IDatabaseNameProvider>()
                .Named(DatabaseNameStrategyRegistrationKeys.YearSpecificOds)
                .ImplementedBy<YearSpecificOdsDatabaseNameProvider>());
        }
    }
}