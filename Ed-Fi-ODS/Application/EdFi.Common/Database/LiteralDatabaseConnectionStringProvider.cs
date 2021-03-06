﻿namespace EdFi.Common.Database
{
    /// <summary>
    /// Provides a connection string that is specified as a literal string value provided in the constructor.
    /// </summary>
    public class LiteralDatabaseConnectionStringProvider : IDatabaseConnectionStringProvider
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralDatabaseConnectionStringProvider"/> class using the specified connection string.
        /// </summary>
        /// <param name="connectionString">The literal connection string to provide to consumers.</param>
        public LiteralDatabaseConnectionStringProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Returns the literal connection string supplied in the constructor
        /// </summary>
        /// <returns>The literal connection string.</returns>
        public string GetConnectionString()
        {
            return _connectionString;
        }
    }
}