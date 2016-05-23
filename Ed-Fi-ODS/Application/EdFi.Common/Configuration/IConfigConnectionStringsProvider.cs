namespace EdFi.Common.Configuration
{
    /// <summary>
    /// Defines a facade for working with the connection strings section of an application configuration file.
    /// </summary>
    public interface IConfigConnectionStringsProvider
    {
        /// <summary>
        /// Gets the number of connection strings defined.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Get a configured connection string by name.
        /// </summary>
        /// <param name="name">The name of the connection string.</param>
        /// <returns>The configured connection string.</returns>
        string GetConnectionString(string name);
    }
}