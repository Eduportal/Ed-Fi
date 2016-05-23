using System;

using EdFi.Common.Database;

namespace EdFi.Ods.Entities.Common.UniqueIdIntegration
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;

    /// <summary>
    /// Creates a persistent mapping between a supplied UniqueId value and a GUID-based identifier.
    /// </summary>
    public class UniqueIdPersonMappingFactory : IUniqueIdPersonMappingFactory
    {
        private readonly IDatabaseConnectionStringProvider uniqueIdIntegrationDatabaseConnectionStringProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueIdPersonMappingFactory"/> class 
        /// using the specified UniqueId integration database connection string provider.
        /// </summary>
        /// <param name="uniqueIdIntegrationDatabaseConnectionStringProvider">The connection string
        /// provider used to obtain a connection string to the database where the UniqueIdPersonMapping
        /// table is located.</param>
        public UniqueIdPersonMappingFactory(IDatabaseConnectionStringProvider uniqueIdIntegrationDatabaseConnectionStringProvider)
        {
            this.uniqueIdIntegrationDatabaseConnectionStringProvider = uniqueIdIntegrationDatabaseConnectionStringProvider;
        }

        /// <summary>
        /// Creates a persistent mapping between the supplied UniqueId value and GUID-based identifier.
        /// </summary>
        /// <param name="uniqueId">The UniqueId of the person.</param>
        /// <param name="id">The associated GUID-based identifier.</param>
        public void CreateMapping(string uniqueId, Guid id)
        {
            var connectionString = uniqueIdIntegrationDatabaseConnectionStringProvider.GetConnectionString();
            using (var db = new UniqueIdIntegrationContext(connectionString))
            {
                db.UniqueIdPersonMappings.Add(new UniqueIdPersonMapping { Id = id, UniqueId = uniqueId });
                db.SaveChanges();
            }
        }
    }

    public class UniqueIdIntegrationContext : DbContext
    {
        public UniqueIdIntegrationContext(string connectionString)
            : base(connectionString)
        {
        }

        public UniqueIdIntegrationContext()
            : base()
        {
        }

        public DbSet<UniqueIdPersonMapping> UniqueIdPersonMappings { get; set; }
    }


    public class UniqueIdPersonMapping
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(32)]
        public string UniqueId { get; set; }
    }
}