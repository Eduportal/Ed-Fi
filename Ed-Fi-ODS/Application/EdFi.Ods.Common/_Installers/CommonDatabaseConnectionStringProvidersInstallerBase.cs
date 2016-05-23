using System;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EdFi.Common.Database;
using EdFi.Common.InversionOfControl;
using EdFi.Ods.Common._Installers.ComponentNaming;

namespace EdFi.Ods.Common._Installers
{
    /// <summary>
    /// Provides an abstract base class with the registration and software configuration needed
    /// to support database connections used by the EdFi.Ods solution.
    /// </summary>
    public abstract class CommonDatabaseConnectionStringProvidersInstallerBase 
        : RegistrationMethodsInstallerBase 
    {
        /// <summary>
        /// Registers the database connection string provider for the ODS database.
        /// </summary>
        /// <param name="container">The Castle Windsor container.</param>
        protected abstract void RegisterOdsDatabaseConnectionStringProvider(IWindsorContainer container);

        /// <summary>
        /// Registers the database connection string provider for the database containing the UniqueIdMapping table.
        /// </summary>
        /// <param name="container">The Castle Windsor container.</param>
        protected virtual void RegisterUniqueIdIntegratioDatabaseConnectionStringProvider(IWindsorContainer container)
        {
            // UniqueId Person mapping database
            RegisterNamedConnectionStringProvider(container, Databases.UniqueIdIntegration);
        }

        /// <summary>
        /// Registers the database connection string provider for the Admin database.
        /// </summary>
        /// <param name="container">The Castle Windsor container.</param>
        protected virtual void RegisterAdminDatabaseConnectionStringProvider(IWindsorContainer container)
        {
            // Admin database
            RegisterNamedConnectionStringProvider(container, Databases.Admin);
        }

        /// <summary>
        /// Registers the database connection string provider for the master database for creating sandboxes.
        /// </summary>
        /// <param name="container">The Castle Windsor container.</param>
        protected virtual void RegisterMasterDatabaseConnectionStringProvider(IWindsorContainer container)
        {
            // Master database conection (for creating sandboxes)
            RegisterNamedConnectionStringProvider(container, Databases.Master);
        }

        /// <summary>
        /// Registers the database connection string provider for the EduId database.
        /// </summary>
        /// <param name="container">The Castle Windsor container.</param>
        protected virtual void RegisterEduIdDatabaseConnectionStringProvider(IWindsorContainer container)
        {
            // EduId database (TODO: Code Split: This will need to be move to TN-specific portion of the codebase)
            RegisterNamedConnectionStringProvider(container, Databases.EduId);
        }

        /// <summary>
        /// Registers the database connection string provider for the database containing information about the Bulk Operations.
        /// </summary>
        /// <param name="container">The Castle Windsor container.</param>
        protected virtual void RegisterBulkOperationsDatabaseConnectionStringProvider(IWindsorContainer container)
        {
            // Bulk Operation database
            RegisterNamedConnectionStringProvider(container, Databases.BulkOperations);
        }

        /// <summary>
        /// Registers the named database connection string provider with the specified database enumeration value (where the
        /// enumerated value defines the connection string name via the <see cref="ConnectionStringNameAttribute"/> class).
        /// </summary>
        /// <typeparam name="TEnum">The type of the enumeration defining the database codes.</typeparam>
        /// <param name="container">The Castle Windsor container.</param>
        /// <param name="databaseCode">The enumerated value identifying the database for which the named connection string provider should be registered.</param>
        protected void RegisterNamedConnectionStringProvider<TEnum>(IWindsorContainer container, TEnum databaseCode)
        {
            string fieldName = Enum.GetName(typeof(TEnum), databaseCode);

            string connectionStringName = typeof(TEnum).GetField(fieldName)
                .GetCustomAttributes(typeof(ConnectionStringNameAttribute), false)
                .Cast<ConnectionStringNameAttribute>()
                .Select(x => x.ConnectionStringName)
                .SingleOrDefault();

            if (connectionStringName == null)
                throw new ArgumentException("The enumerated database value supplied does not have a ConnectionStringName attribute applied, and no connection string name was supplied.  Use the other overload, or define the attribute on the enumerated value.", "databaseCode");

            RegisterNamedConnectionStringProvider(container, databaseCode, connectionStringName);
        }

        /// <summary>
        /// Registers the named database connection string provider with the specified database enumeration value
        /// and connection string name.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enumeration defining the database codes.</typeparam>
        /// <param name="container">The Castle Windsor container.</param>
        /// <param name="databaseCode">The enumerated value identifying the database for which the named connection string provider should be registered.</param>
        /// <param name="connectionStringName">The name of the configured connection string.</param>
        public static void RegisterNamedConnectionStringProvider<TEnum>(
            IWindsorContainer container,
            TEnum databaseCode,
            string connectionStringName)
        {
            container.Register(
                Component
                    .For<IDatabaseConnectionStringProvider>()
                    .NamedForDatabase(databaseCode)
                    .ImplementedBy<NamedDatabaseConnectionStringProvider>()
                    .DependsOn(Dependency.OnValue("connectionStringName", connectionStringName))
            );
        }
    }
}
