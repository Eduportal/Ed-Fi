using System;
using EdFi.Common.Database;
using NHibernate.Connection;
using System.Data;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    /// <summary>
    /// An NHibernate <see cref="ConnectionProvider"/> implementation that uses the registered ODS 
    /// connection string provider (using the key "IDatabaseConnectionStringProvider.Ods") to obtain 
    /// the connection string for opening a connection to the database.
    /// </summary>
    public class EdFiOdsConnectionProvider : DriverConnectionProvider
    {
        private readonly IDatabaseConnectionStringProvider _connectionStringProvider;

        public EdFiOdsConnectionProvider(IDatabaseConnectionStringProvider connectionStringProvider)
        {
            _connectionStringProvider = connectionStringProvider;
        }

        /// <summary>
        /// Create and open a connection to the Ed-Fi ODS for NHibernate.
        /// </summary>
        /// <returns>An open database connection.</returns>
        public override IDbConnection GetConnection()
        {
            var connection = Driver.CreateConnection();
            try
            {
                connection.ConnectionString = _connectionStringProvider.GetConnectionString();
                connection.Open();
            }
            catch (Exception)
            {
                connection.Dispose();
                throw;
            }
            return connection;
        }
    }
}
