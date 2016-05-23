// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************
using EdFi.Ods.Common._Installers.ComponentNaming;

namespace EdFi.Ods.Api._Installers
{
    /// <summary>
    /// Provides short names (and connection string names via field-level attributes) for databases 
    /// used by the application.
    /// </summary>
    public enum UniqueIdDatabases
    {
        [ConnectionStringName("UniqueIdIntegrationContext")]
        UniqueIdIntegration,
    }
}