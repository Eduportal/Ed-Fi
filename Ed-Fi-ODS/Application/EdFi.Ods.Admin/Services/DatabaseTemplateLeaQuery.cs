using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using EdFi.Ods.Admin.Models;

namespace EdFi.Ods.Admin.Services
{
    public class DatabaseTemplateLeaQuery : IDatabaseTemplateLeaQuery
    {
        private const string MinimalConnectionString = "EdFi_Ods_Minimal_Template";
        private const string PopulatedConnectionString = "EdFi_Ods_Populated_Template";

        public int[] GetAllMinimalTemplateLeaIds()
        {
            return GetLeasForConnectionString(MinimalConnectionString);
        }

        public int[] GetAllPopulatedTemplateLeaIds()
        {
            return GetLeasForConnectionString(PopulatedConnectionString);
        }

        private static int[] GetLeasForConnectionString(string connectionStringKey)
        {
            var results = new List<int>();

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringKey];
            if (connectionStringSettings == null)
                throw new ConfigurationErrorsException(string.Format("The connection string [{0}] is required.",
                                                                     connectionStringKey));
            var connectionString = connectionStringSettings.ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var sql = "select LocalEducationAgencyId from edfi.LocalEducationAgency";
                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add((int) reader[0]);
                        }
                    }
                }
            }
            return results.ToArray();
        }

        public int[] GetLocalEducationAgencyIds(SandboxType sandboxType)
        {
            switch (sandboxType)
            {
                case SandboxType.Sample:
                    return GetAllPopulatedTemplateLeaIds();
                case SandboxType.Minimal:
                    return GetAllMinimalTemplateLeaIds();
                case SandboxType.Empty:
                    return new int[0];
                default:
                    throw new Exception(string.Format("Cannot lookup LEA's for sandbox type {0}", sandboxType));
            }
        }
    }
}