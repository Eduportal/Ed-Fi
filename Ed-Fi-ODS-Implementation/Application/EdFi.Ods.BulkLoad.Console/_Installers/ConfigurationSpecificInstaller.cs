using Castle.Windsor;
using Castle.MicroKernel.Registration;
using EdFi.Common.Caching;
using EdFi.Common.Context;
using EdFi.Common.InversionOfControl;
using EdFi.Common.Security.Authorization;
using EdFi.Common.Security.Claims;
using EdFi.Common._Installers;
using EdFi.Ods.Api.Pipelines._Installers;
using EdFi.Ods.BulkLoad.Core._Installers;
using EdFi.Ods.Common;
using EdFi.Ods.Common._Installers;
using EdFi.Ods.Entities.Repositories.NHibernate._Installers;
using EdFi.Ods.XmlShredding._Installers;
using EdFi.Ods.Security.Metadata.Repositories;

namespace EdFi.Ods.BulkLoad.Console._Installers
{
    /// <summary>
    /// Registers components for the EdFi.Ods.BulkLoad.Console assembly.
    /// </summary>
    public class ConfigurationSpecificInstaller : CommonConfigurationSpecificInstallerBase
    {
        // Add registrations for components that may have alternative implementations
        // based on different environmental configurations or implementation-specific requirements
        // protected virtual void RegisterISomething(IWindsorContainer container)
        // {
        //     container.Register(Component
        //            .For<ISomething>()
        //            .ImplementedBy<Something>());
        // }

        // -------------------------------------------------
        //   Configuration-Specific Override Registrations
        // -------------------------------------------------
        // TODO: Override common decisions, or register other unnamed components to override with Castle (first in wins)
        protected override void RegisterIContextStorage(IWindsorContainer container)
        {
            container.Register(Component
                .For<IContextStorage>()
                .ImplementedBy<CallContextStorage>());
        }

        protected override void RegisterICacheProvider(IWindsorContainer container)
        {
            container.Register(Component
                .For<ICacheProvider>()
                .ImplementedBy<MemoryCacheProvider>());
        }

        protected override void RegisterDatabaseConnectionStringProviders(IWindsorContainer container)
        {
            // Connection string providers for the databases used by the bulk load console program
            // are installed in the IoC factory initialization delegate because the connection
            // string is based on runtime command-line arguments.
        }

        // --------------------------------------
        //   Register Local Assembly Components 
        // --------------------------------------
        protected virtual void RegisterIBulkExecutor(IWindsorContainer container)
        {
            container.Register(Component
                .For<IBulkExecutor>()
                .ImplementedBy<BulkLoadExecutor>());
        }

        protected virtual void RegisterIValidateAndSourceFiles(IWindsorContainer container)
        {
            //container.Register(Component
            //    .For<IValidateAndSourceFiles>()
            //    .ImplementedBy<ValidateAndSourceLocalOnlyFiles>()
            //    .IsDefault()); // Interface is externally defined, so must be marked as default
        }

        protected virtual void RegisterIConfigurationAccess(IWindsorContainer container)
        {
            container.Register(Component
                .For<IConfigurationAccess>()
                .ImplementedBy<ConfigurationAccess>());
        }

        // --------------------------------------------
        //   Install Referenced Assembly Dependencies
        // --------------------------------------------
        protected virtual void RegisterEdFiOdsBulkLoadCoreDependencies(IWindsorContainer container)
        {
            container.Install(new EdFiOdsBulkLoadCoreInstaller());
        }

        protected virtual void RegisterEdFiCommonDependencies(IWindsorContainer container)
        {
            container.Install(new EdFiCommonInstaller());
        }

        protected virtual void RegisterEdFiOdsRepositoriesNHibernateDependencies(IWindsorContainer container)
        {
            container.Install(new EdFiOdsRepositoriesNHibernateInstaller());
        }

        protected virtual void RegisterEdFiOdsXmlShreddingDependencies(IWindsorContainer container)
        {
            container.Install(new EdFiOdsXmlShreddingInstaller());
        }

        protected virtual void RegisterSecurityRepository(IWindsorContainer container)
        {
            container.Register(Component
                .For<ISecurityRepository>()
                .ImplementedBy<InMemorySecurityRepository>()
                .LifestyleTransient()
                .OnlyNewServices());
        }

        protected virtual void RegisterAuthorizationContextDependencies(IWindsorContainer container)
        {
            container.Register(Component
                .For<IAuthorizationContextProvider>()
                .ImplementedBy<AuthorizationContextProvider>());
        }

        // -----------------------------------
        //   Register Theme-based Components
        // -----------------------------------
        // e.g. Infrastructure (Azure/Amazon/Windows), Security components, etc.
        // TODO: Register components that need to be registered together to function correctly

        // --------------------------------------
        //   Classification-based Registrations
        // --------------------------------------
        // e.g. Controllers, Repositories, Services, etc. 
        // TODO: Register all components of a particular type
    }
}