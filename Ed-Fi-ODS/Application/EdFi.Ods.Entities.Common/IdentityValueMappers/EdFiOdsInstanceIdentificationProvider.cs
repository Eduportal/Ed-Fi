using EdFi.Common.Database;

namespace EdFi.Ods.Entities.Common.Providers
{
    /// <summary>
    /// Defines an interface for providing a unique identification value to be used to 
    /// delineate different ODS instances.
    /// </summary>
    /// <remarks>This interface and its implementations are used to facilitate caching of
    /// values that are unique to a particular ODS.  For example, different sandbox ODSs may be
    /// based on the "minimal" template or the "populated" template with different types and
    /// descriptors values, and when there are year-specific ODSs, they will have different 
    /// USI values for people.</remarks>
    public interface IEdFiOdsInstanceIdentificationProvider
    {
        /// <summary>
        /// Gets a unique identifying value for the ODS currently in context.
        /// </summary>
        /// <returns>A unique integer value (likely the GetHashCode value for the connection string).</returns>
        int GetInstanceIdentification();
    }

    /// <summary>
    /// Implements an ODS identification provider that returns unique values based on the 
    /// GetHashCode return value for the ODS connection string currently in context.
    /// </summary>
    public class EdFiOdsInstanceIdentificationProvider : IEdFiOdsInstanceIdentificationProvider
    {
        private readonly IDatabaseConnectionStringProvider odsDatabaseConnectionStringProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdFiOdsInstanceIdentificationProvider"/> class
        /// the specified ODS database connection string provider.
        /// </summary>
        /// <param name="odsDatabaseConnectionStringProvider">The ODS connection string provider.</param>
        public EdFiOdsInstanceIdentificationProvider(IDatabaseConnectionStringProvider odsDatabaseConnectionStringProvider)
        {
            this.odsDatabaseConnectionStringProvider = odsDatabaseConnectionStringProvider;
        }

        /// <summary>
        /// Gets the identification value for the ODS currently in context.
        /// </summary>
        /// <returns>The identification value.</returns>
        public int GetInstanceIdentification()
        {
            return odsDatabaseConnectionStringProvider.GetConnectionString().GetHashCode();
        }
    }
}
