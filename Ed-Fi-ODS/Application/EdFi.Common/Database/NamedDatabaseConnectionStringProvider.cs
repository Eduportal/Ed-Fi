using System;
using System.Configuration;

namespace EdFi.Common.Database
{
    /// <summary>
    /// Provides a connection string that is identified as a named connection string from 
    /// the application configuration file's 'connectionStrings' section.
    /// </summary>
    public class NamedDatabaseConnectionStringProvider : IDatabaseConnectionStringProvider
    {
        private readonly string connectionStringName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedDatabaseConnectionStringProvider"/> 
        /// class using the specified connection string name (matching a connection string defined
        /// in the application's configuration file's 'connectionStrings' section
        /// </summary>
        /// <param name="connectionStringName">The name of the connection string.</param>
        public NamedDatabaseConnectionStringProvider(string connectionStringName)
        {
            this.connectionStringName = connectionStringName;
        }

        /// <summary>
        /// Gets the named connection string from the application configuration file's 'connectionStrings' section.
        /// </summary>
        /// <returns>The connection string.</returns>
        public string GetConnectionString()
        {
            if (ConfigurationManager.ConnectionStrings.Count == 0)
                throw new ConfigurationErrorsException("No connection strings were found in the configuration file.");

            if(string.IsNullOrWhiteSpace(connectionStringName))
                throw new ArgumentNullException("connectionStringName");

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (connectionStringSettings == null)
                throw new ArgumentNullException(string.Format("Could not find connection string named [{0}]", connectionStringName));
            return connectionStringSettings.ConnectionString;
        }
    }
}
