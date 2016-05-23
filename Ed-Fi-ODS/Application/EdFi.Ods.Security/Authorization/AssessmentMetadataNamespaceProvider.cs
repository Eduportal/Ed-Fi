using EdFi.Common.Database;
using System.Data;
using System.Data.SqlClient;

namespace EdFi.Ods.Security.Authorization
{
    public interface IAssessmentMetadataNamespaceProvider
    {
        string GetNamespaceByAssessmentFamilyTitle(string assessmentFamilyTitle);
        string GetNamespaceByAssessmentTitle(string assessmentTitle);
    }

    public class AssessmentMetadataNamespaceProvider : IAssessmentMetadataNamespaceProvider
    {
        private readonly IDatabaseConnectionStringProvider odsDatabaseConnectionStringProvider;

        public AssessmentMetadataNamespaceProvider(IDatabaseConnectionStringProvider odsDatabaseConnectionStringProvider)
        {
            this.odsDatabaseConnectionStringProvider = odsDatabaseConnectionStringProvider;
        }

        public string GetNamespaceByAssessmentFamilyTitle(string assessmentFamilyTitle)
        {
            string retVal = null;

            string sql = string.Format(@"
SELECT    assessmentFamily.Namespace
FROM    edfi.AssessmentFamily assessmentFamily
WHERE    assessmentFamily.AssessmentFamilyTitle = '{0}'", assessmentFamilyTitle);

            using (var conn = new SqlConnection(odsDatabaseConnectionStringProvider.GetConnectionString()))
            {
                conn.Open();
                
                var cmd = new SqlCommand(sql, conn);

                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        var Namespace = reader.GetOrdinal("Namespace");
                        retVal = (!reader.IsDBNull(Namespace)) ? reader.GetString(Namespace) : null;
                    }
                }
            }

            return retVal;
        }

        public string GetNamespaceByAssessmentTitle(string assessmentTitle)
        {
            string assessmentNamespace = null;
            string familyNamespace = null;

            string sql = string.Format(@"
SELECT    assessment.Namespace as AssessmentNamespace, assessmentFamily.Namespace as FamilyNamespace
FROM    edfi.Assessment assessment LEFT OUTER JOIN edfi.AssessmentFamily assessmentFamily ON assessmentFamily.AssessmentFamilyTitle = assessment.AssessmentFamilyTitle
WHERE    assessment.AssessmentTitle = '{0}'", assessmentTitle);

            using (var conn = new SqlConnection(odsDatabaseConnectionStringProvider.GetConnectionString()))
            {
                conn.Open();

                var cmd = new SqlCommand(sql, conn);

                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        var assessmentNamespaceOrdinal = reader.GetOrdinal("AssessmentNamespace");
                        assessmentNamespace = (!reader.IsDBNull(assessmentNamespaceOrdinal)) ? reader.GetString(assessmentNamespaceOrdinal) : null;

                        var familyNamespaceOrdinal = reader.GetOrdinal("FamilyNamespace");
                        familyNamespace = (!reader.IsDBNull(familyNamespaceOrdinal)) ? reader.GetString(familyNamespaceOrdinal) : null;
                    }
                }
            }

            return (!string.IsNullOrWhiteSpace(assessmentNamespace)) ? assessmentNamespace : familyNamespace;
        }
    }
}
