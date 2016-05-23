using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Caching;
using EdFi.Common.Context;
using EdFi.Ods.Common._Installers;

namespace EdFi.Workers.BulkLoad._Installers
{
    /// <summary>
    /// Registers components for the EdFi.Workers.BulkLoad assembly.
    /// </summary>
    public abstract class ConfigurationSpecificInstallerBase : CommonConfigurationSpecificInstallerBase
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
        
        // --------------------------------------
        //   Register Local Assembly Components 
        // --------------------------------------
        // TODO: Register components defined locally (even if the interfaces are located elsewhere)

        // --------------------------------------------
        //   Install Referenced Assembly Dependencies
        // --------------------------------------------
        // TODO: Invoke installers for referenced assemblies
        //
        // protected virtual void Register{NamespaceWithoutDots}Dependencies(IWindsorContainer container)
        // {
        //    container.Install(new {NamespaceWithoutDots}Installer());
        // }

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