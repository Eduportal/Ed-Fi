using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using EdFi.Ods.SecurityConfiguration.Services.Model;

namespace EdFi.Ods.SecurityConfiguration.Services.Implementation
{
    public class EducationOrganizationProvider : IEducationOrganizationProvider
    {
        private readonly IOdsConnectionStringProvider _odsConnectionStringProvider;

        public EducationOrganizationProvider(IOdsConnectionStringProvider odsConnectionStringProvider)
        {
            _odsConnectionStringProvider = odsConnectionStringProvider;
        }

        public IEnumerable<EducationOrganization> GetAll()
        {
            return GetLocalEducationAgencies().Concat(GetSchools());
        }

        public IEnumerable<EducationOrganization> GetLocalEducationAgencies()
        {
            var odsConnectionStrings = _odsConnectionStringProvider.GetOdsConnectionStrings();

            var localEducationAgencies = new List<EducationOrganization>();

            foreach (var connectionStringSettings in odsConnectionStrings)
            {
                localEducationAgencies = localEducationAgencies.Union(GetLocalEducationAgencies(connectionStringSettings)).ToList();
            }

            return localEducationAgencies;
        }

        public IEnumerable<EducationOrganization> GetSchools()
        {
            var odsConnectionStrings = _odsConnectionStringProvider.GetOdsConnectionStrings();

            var schools = new List<EducationOrganization>();

            foreach (var connectionStringSettings in odsConnectionStrings)
            {
                schools = schools.Union(GetSchools(connectionStringSettings)).ToList();
            }

            return schools;
        }

        private static IEnumerable<EducationOrganization> GetLocalEducationAgencies(ConnectionStringSettings connectionStringSettings)
        {
            const string commandText = @"
                select LocalEducationAgencyId, NameOfInstitution from 
                    edfi.LocalEducationAgency lea 
                    inner join edfi.EducationOrganization edorg
                on lea.LocalEducationAgencyId = edorg.EducationOrganizationId
                ";
            using (var conn = new SqlConnection(connectionStringSettings.ConnectionString))
            using (var command = new SqlCommand(commandText, conn))
            using (var dataAdapter = new SqlDataAdapter(command))
            {
                conn.Open();

                var leaTable = new DataTable();
                dataAdapter.Fill(leaTable);

                return leaTable.AsEnumerable().Select(row => new EducationOrganization
                {
                    EducationOrganizationId = (int) row["LocalEducationAgencyId"],
                    EducationOrganizationName = (string) row["NameOfInstitution"]
                });
            }
        }

        private static IEnumerable<EducationOrganization> GetSchools(ConnectionStringSettings connectionStringSettings)
        {
            const string commandText = @"
                select SchoolId, NameOfInstitution from 
                    edfi.School school
                    inner join edfi.EducationOrganization edorg
                on school.SchoolId = edorg.EducationOrganizationId
                ";
            using (var conn = new SqlConnection(connectionStringSettings.ConnectionString))
            using (var command = new SqlCommand(commandText, conn))
            using (var dataAdapter = new SqlDataAdapter(command))
            {
                conn.Open();

                var leaTable = new DataTable();
                dataAdapter.Fill(leaTable);

                return leaTable.AsEnumerable().Select(row => new EducationOrganization
                {
                    EducationOrganizationId = (int) row["SchoolId"],
                    EducationOrganizationName = (string) row["NameOfInstitution"]
                });
            }
        }
    }
}
