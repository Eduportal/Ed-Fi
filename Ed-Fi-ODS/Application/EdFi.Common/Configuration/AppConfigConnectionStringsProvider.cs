using System.Configuration;

namespace EdFi.Common.Configuration
{
    /// <summary>
    /// Provides access to the configured connections strings using the <see cref="ConfigurationManager"/>.
    /// </summary>
    public class AppConfigConnectionStringsProvider : IConfigConnectionStringsProvider
    {
        /// <summary>
        /// Gets the number of connection strings defined.
        /// </summary>
        public int Count 
        {
            get { return ConfigurationManager.ConnectionStrings.Count; }
        }

        /// <summary>
        /// Get a configured connection string by name.
        /// </summary>
        /// <param name="name">The name of the connection string.</param>
        /// <returns>The configured connection string.</returns>
        public string GetConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }
    }
}
