using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Configuration;
using EdFi.Common.InversionOfControl;

namespace EdFi.Ods.Common._Installers
{
    /// <summary>
    /// Provides an abstract base class with the specific decision points that must be made by
    /// all applications.
    /// </summary>
    public abstract class CommonConfigurationSpecificInstallerBase : RegistrationMethodsInstallerBase
    {
        // Add registrations for components that may have alternative implementations
        // based on different environmental configurations or implementation-specific requirements
        // protected virtual void RegisterISomething(IWindsorContainer container)
        // {
        //     container.Register(Component
        //            .For<ISomething>()
        //            .ImplementedBy<Something>());
        // }

        [Preregister] // Preregistration prioritizes these registrations so that other registration activities can resolve and use them
        protected virtual void RegisterIConfigConnectionStringsProvider(IWindsorContainer container)
        {
            container.Register(Component
                .For<IConfigConnectionStringsProvider>()
                .ImplementedBy<AppConfigConnectionStringsProvider>());
        }

        [Preregister] // Preregistration prioritizes these registrations so that other registration activities can resolve and use them
        protected virtual void RegisterIConfigValueProvider(IWindsorContainer container)
        {
            container.Register(Component
                .For<IConfigValueProvider>()
                .ImplementedBy<AppConfigValueProvider>());
        }

        [Preregister] // Preregistration prioritizes these registrations so that other registration activities can resolve and use them
        protected virtual void RegisterConfigSectionProvider(IWindsorContainer container)
        {
            container.Register(Component
                .For<IConfigSectionProvider>()
                .ImplementedBy<AppConfigSectionProvider>());
        }

        /// <summary>
        /// Defines a method that forces a decision about the appropriate context storage mechanism for an application.
        /// </summary>
        /// <param name="container">The Castle Windsor container on which to perform registrations.</param>
        protected abstract void RegisterIContextStorage(IWindsorContainer container);

        /// <summary>
        /// Defines a method that forces a decision about the appropriate cache provider for an application.
        /// </summary>
        /// <param name="container">The Castle Windsor container on which to perform registrations.</param>
        protected abstract void RegisterICacheProvider(IWindsorContainer container);

        // -----------------------------
        //   Theme-based Registrations
        // -----------------------------
        // e.g. Security, Database Connection Strings, etc.

        /// <summary>
        /// Defines a method that allows for the registration of database connection string access for an application.
        /// </summary>
        /// <param name="container">The Castle Windsor container on which to perform registrations.</param>
        protected abstract void RegisterDatabaseConnectionStringProviders(IWindsorContainer container);

        // --------------------------------------
        //   Classification-based Registrations
        // --------------------------------------
        // e.g. Controllers, Repositories, Services, etc.
    }
}