using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Database;
using EdFi.Ods.Common.Database;
using EdFi.Ods.Common._Installers.ComponentNaming;

namespace EdFi.Ods.Common._Installers
{
    /// <summary>
    /// Installs database connection providers that are handled differently for the 
    /// sandbox environment.
    /// </summary>
    public class SandboxEnvironmentDatabaseConnectionStringsInstaller
        : CommonDatabaseConnectionStringProvidersInstallerBase
    {
        protected override void RegisterOdsDatabaseConnectionStringProvider(IWindsorContainer container)
        {
            // For ODS Sandboxes
            container.Register(Component
                .For<IDatabaseConnectionStringProvider>()
                .NamedForDatabase(Databases.Ods)
                .ImplementedBy<PrototypeWithDatabaseNameOverrideDatabaseConnectionStringProvider>()
                .DependsOn(Dependency.OnValue("prototypeConnectionStringName", "EdFi_Ods"))
                .DependsOn(Dependency
                    .OnComponent(
                        typeof(IDatabaseNameProvider),
                        DatabaseNameStrategyRegistrationKeys.Sandbox))
            );
        }

        protected virtual void RegisterISandboxDatabaseNameProvider(IWindsorContainer container)
        {
            // Builds the sandbox database name
            container.Register(Component
                .For<IDatabaseNameProvider>()
                .ImplementedBy<SandboxDatabaseNameProvider>()
                .Named(DatabaseNameStrategyRegistrationKeys.Sandbox));
        }
    }
}