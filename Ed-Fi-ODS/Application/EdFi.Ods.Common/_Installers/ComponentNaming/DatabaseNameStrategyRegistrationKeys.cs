using EdFi.Common.Database;

namespace EdFi.Ods.Common._Installers.ComponentNaming
{
    /// <summary>
    /// Provides keys for IoC container registrations for identifying specific implementations
    /// of database name strategies for configuration purposes.
    /// </summary>
    public static class DatabaseNameStrategyRegistrationKeys
    {
        private static readonly string BaseName = typeof(IDatabaseNameProvider).Name + ".";

        /// <summary>
        /// Get the registration key to use when registering or referring to the database naming 
        /// strategy for Ed-Fi ODS Sandbox databases.
        /// </summary>
        public static string Sandbox = BaseName + "Sandbox"; 
        
        /// <summary>
        /// Get the registration key to use when registering or referring to the database naming 
        /// strategy for year-specific Ed-Fi ODS databases.
        /// </summary>
        public static string YearSpecificOds = BaseName + "YearSpecificOds";
    }
}