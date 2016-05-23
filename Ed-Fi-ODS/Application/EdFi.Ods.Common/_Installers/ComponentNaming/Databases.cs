using System;
using System.Data;
using System.Data.SqlClient;
using EdFi.Common.Database;

namespace EdFi.Ods.Common._Installers.ComponentNaming
{
    /// <summary>
    /// Provides short names (and connection string names via field-level attributes) for databases 
    /// used by the application.
    /// </summary>
    public enum Databases
    {
        [ConnectionStringName("EdFi_Ods")]
        Ods,
        
        [ConnectionStringName("EdFi_Admin")]
        Admin,

        [ConnectionStringName("Sso_Integration")]
        SsoIntegration,

        [ConnectionStringName("EduIdContext")]
        EduId,

        [ConnectionStringName("BulkOperationDbContext")]
        BulkOperations,

        [ConnectionStringName("EdFi_master")]
        Master,

        [Obsolete("Use the UniqueIdIntegration enumerated database from the UniqueIdDatabases enumeration.")]
        [ConnectionStringName("UniqueIdIntegrationContext")]
        UniqueIdIntegration,
    }
}