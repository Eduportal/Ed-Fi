using System;
using System.Data.SqlClient;
using System.Globalization;

namespace EdFi.Ods.WebService.Tests.AuthorizationTests
{
    public class DataSeedHelper
    {
        private readonly string _databaseName;

        public static string RandomName
        { get { return DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture); } }

        public DataSeedHelper(string databaseName)
        {
            _databaseName = databaseName;
        }

        public void CreateStateEducationAgency(int stateEducationAgencyId, string nameOfInstitution = null)
        {
            ExecuteCommand(string.Format("insert into edfi.EducationOrganization(EducationOrganizationId, StateOrganizationId, NameOfInstitution, Id) values({0}, {0}, '{1}', newid())", stateEducationAgencyId, nameOfInstitution ?? "State-" + stateEducationAgencyId));
            ExecuteCommand(string.Format("insert into edfi.StateEducationAgency select {0}", stateEducationAgencyId));
        }

        public void CreateLocalEducationAgency(int localEducationAgencyId, int stateEducationAgencyId, string nameOfInstitution = null)
        {
            ExecuteCommand(string.Format("insert into edfi.EducationOrganization(EducationOrganizationId, StateOrganizationId, NameOfInstitution, Id) values({0}, {0}, '{1}', newid())", localEducationAgencyId, nameOfInstitution ?? "LEA-" + localEducationAgencyId));
            ExecuteCommand(string.Format("insert into edfi.LocalEducationAgency (LocalEducationAgencyId, LocalEducationAgencyCategoryTypeId, StateEducationAgencyId) values ({0}, 1, {1})", localEducationAgencyId, stateEducationAgencyId));
        }

        public void CreateSchool(int schoolId, int leaId, string nameOfInstitution = null)
        {
            ExecuteCommand(string.Format("insert into edfi.EducationOrganization(EducationOrganizationId, StateOrganizationId, NameOfInstitution, Id) values({0}, {0}, '{1}', newid())", schoolId, nameOfInstitution ?? "SCH-" + schoolId));
            ExecuteCommand(string.Format("insert into edfi.School (SchoolId, LocalEducationAgencyId) values ({0}, {1})", schoolId, leaId));
        }

        private void ExecuteCommand(string s)
        {
            var connectionString = new SqlConnectionStringBuilder
            {
                DataSource = "localhost",
                InitialCatalog = _databaseName,
                IntegratedSecurity = true
            };

            using (var conn = new SqlConnection(connectionString.ConnectionString))
            {
                try
                {
                    conn.Open();

                    var command = new SqlCommand(s, conn);
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                }
            }

        }
    }
}