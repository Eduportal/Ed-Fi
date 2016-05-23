using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using EdFi.Ods.BulkLoad.Core;

namespace EdFi.Ods.BulkLoad.Console
{
    public class ConfigurationBuilder
    {
        private readonly IConfigurationAccess _configurationAccess;

        public ConfigurationBuilder()
            : this(new ConfigurationAccess())
        {
        }

        public ConfigurationBuilder(IConfigurationAccess configurationAccess)
        {
            _configurationAccess = configurationAccess;
        }

        public BulkLoaderConfiguration BuildConfiguration(Dictionary<TokenType, string> parsedArgs)
        {
            string connectionString= BuildConnectionString(parsedArgs);

            var config = new BulkLoaderConfiguration
            {
                DatabaseNameOverride =
                    parsedArgs.ContainsKey(TokenType.DatabaseName)
                        ? parsedArgs[TokenType.DatabaseName]
                        : string.Empty,
                SourceFolder = parsedArgs.ContainsKey(TokenType.SourceFolder)? parsedArgs[TokenType.SourceFolder] : null,
                OdsConnectionString = connectionString,
                Manifest = parsedArgs.ContainsKey(TokenType.Manifest) ? parsedArgs[TokenType.Manifest] : string.Empty,
            };

            ValidateConfiguration(config);
            return config;
        }

        private string BuildConnectionString(Dictionary<TokenType, string> parsedArgs)
        {
            // Full connection string provided
            if (parsedArgs.ContainsKey(TokenType.ConnectionString))
                return (new SqlConnectionStringBuilder(parsedArgs[TokenType.ConnectionString])).ConnectionString;

            // Use the configured connection string
            var connectionStringBuilder = new SqlConnectionStringBuilder(_configurationAccess.BaseOdsConnectionString);
            
            // Override the database name with command-line argument
            if (parsedArgs.ContainsKey(TokenType.DatabaseName))
                connectionStringBuilder.InitialCatalog = parsedArgs[TokenType.DatabaseName];
            
            return connectionStringBuilder.ConnectionString;
        }

        private static void ValidateConfiguration(BulkLoaderConfiguration config)
        {
            if (!config.HasDatabaseName)
                throw new ArgumentException("If database name option is used, the connection string must be specified.");

            if (string.IsNullOrEmpty(config.SourceFolder))
                throw new ArgumentException("Source folder must be specified.");

            if (!Directory.Exists(config.SourceFolder))
                throw new ArgumentException(string.Format("Source folder \"{0}\" not found", config.SourceFolder));

            if(string.IsNullOrWhiteSpace(config.Manifest))
                throw new ArgumentException("Manifest must be specified with /m [Manifest]");
        }
    }
}