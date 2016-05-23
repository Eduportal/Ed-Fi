using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data.SqlClient;
using EdFi.Common.Configuration;
using EdFi.Common.Database;

namespace EdFi.Ods.Common.Database
{
    /// <summary>
    /// Gets the connection string using a configured named connection string as a prototype for the connection string
    /// with an injected <see cref="IDatabaseNameProvider"/> to override the database name.
    /// </summary>
    public class PrototypeWithDatabaseNameOverrideDatabaseConnectionStringProvider : IDatabaseConnectionStringProvider
    {
        private readonly string _prototypeConnectionStringName;
        private readonly IDatabaseNameProvider _databaseNameProvider;
        private readonly IConfigConnectionStringsProvider _configConnectionStringsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrototypeWithDatabaseNameOverrideDatabaseConnectionStringProvider"/> class using
        /// the specified "prototype" named connection string from the application configuration file and the supplied database name provider.
        /// </summary>
        /// <param name="prototypeConnectionStringName">The named connection string to use as the basis for building the connection string.</param>
        /// <param name="databaseNameProvider">The provider that builds the database name for use in the resulting connection string.</param>
        public PrototypeWithDatabaseNameOverrideDatabaseConnectionStringProvider(string prototypeConnectionStringName, IDatabaseNameProvider databaseNameProvider, IConfigConnectionStringsProvider configConnectionStringsProvider)
        {
            _prototypeConnectionStringName = prototypeConnectionStringName;
            _databaseNameProvider = databaseNameProvider;
            _configConnectionStringsProvider = configConnectionStringsProvider;
        }

        private string _prototypeConnectionString;

        private string PrototypeConnectionString
        {
            get
            {
                if (_prototypeConnectionString == null)
                {
                    if (_configConnectionStringsProvider.Count == 0)
                        throw new ConfigurationErrorsException("No connection strings were found in the configuration file.");

                    if (string.IsNullOrWhiteSpace(_prototypeConnectionStringName))
                        throw new ArgumentNullException("prototypeConnectionStringName");

                    var connectionString = _configConnectionStringsProvider.GetConnectionString(_prototypeConnectionStringName);

                    if (connectionString == null)
                        throw new ConfigurationErrorsException(string.Format("No connection string named '{0}' was found in the 'connectionStrings' section of the application configuration file.", _prototypeConnectionStringName));

                    _prototypeConnectionString = connectionString;
                }

                return _prototypeConnectionString;
            }
        }

        private readonly ConcurrentDictionary<Tuple<string, string>, string> _connectionStringsByNames 
            = new ConcurrentDictionary<Tuple<string, string>, string>(); 

        /// <summary>
        /// Gets the connection string using a configured named connection string with the database replaced using the specified database name provider.
        /// </summary>
        /// <returns>The connection string.</returns>
        public string GetConnectionString()
        {
            string databaseName = _databaseNameProvider.GetDatabaseName();

            return _connectionStringsByNames.GetOrAdd(Tuple.Create(_prototypeConnectionStringName, databaseName), x =>
            {
                var builder = new SqlConnectionStringBuilder(PrototypeConnectionString);

                // Override the Database Name
                builder.InitialCatalog = databaseName;

                return builder.ConnectionString;
            });
        }
    }
}