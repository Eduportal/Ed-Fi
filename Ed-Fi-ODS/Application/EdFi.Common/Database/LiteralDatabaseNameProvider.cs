namespace EdFi.Common.Database
{
    /// <summary>
    /// Provides an <see cref="IDatabaseNameProvider"/> implementation that always returns the database
    /// name provided in the constructor.
    /// </summary>
    public class LiteralDatabaseNameProvider : IDatabaseNameProvider
    {
        private readonly string databaseName;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralDatabaseNameProvider"/> class using the
        /// supplied database name.
        /// </summary>
        /// <param name="databaseName">The database name to return from the <see cref="GetDatabaseName"/> method.</param>
        public LiteralDatabaseNameProvider(string databaseName)
        {
            this.databaseName = databaseName;
        }

        /// <summary>
        /// Gets the database name supplied in the constructor.
        /// </summary>
        /// <returns>The database name.</returns>
        public string GetDatabaseName()
        {
            return databaseName;
        }
    }
}