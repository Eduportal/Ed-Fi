namespace EdFi.Common.Database
{
    /// <summary>
    /// Defines an interface for creating a database name (for inclusion in a connection string).
    /// </summary>
    public interface IDatabaseNameProvider
    {
        /// <summary>
        /// Gets the database name.
        /// </summary>
        /// <returns>The database name.</returns>
        string GetDatabaseName();
    }
}
