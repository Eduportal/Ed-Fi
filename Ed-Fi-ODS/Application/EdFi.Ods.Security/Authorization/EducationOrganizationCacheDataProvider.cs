using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using EdFi.Common.Database;
using EdFi.Common.Security;

namespace EdFi.Ods.Security.Authorization
{
    public interface IEducationOrganizationCacheDataProvider
    {
        List<EducationOrganizationIdentifiers> GetEducationOrganizationIdentifiers();
    }

    public class EducationOrganizationCacheDataProvider : IEducationOrganizationCacheDataProvider
    {
        private readonly IDatabaseConnectionStringProvider _odsDatabaseConnectionStringProvider;

        public EducationOrganizationCacheDataProvider(IDatabaseConnectionStringProvider odsDatabaseConnectionStringProvider)
        {
            _odsDatabaseConnectionStringProvider = odsDatabaseConnectionStringProvider;
        }

        public List<EducationOrganizationIdentifiers> GetEducationOrganizationIdentifiers()
        {
            using (var conn = new SqlConnection(_odsDatabaseConnectionStringProvider.GetConnectionString()))
            {
                conn.Open();

                // TODO: GKM 3/19 - Replace this SQL with [auth].[EducationOrganizationIdentifiers]?  
                string sql = @"
SELECT    edorg.EducationOrganizationId,
        CASE 
            WHEN sea.StateEducationAgencyId IS NOT NULL THEN 'StateEducationAgency'
            WHEN esc.EducationServiceCenterId IS NOT NULL THEN 'EducationServiceCenter'
            WHEN lea.LocalEducationAgencyId IS NOT NULL THEN 'LocalEducationAgency'
            WHEN sch.SchoolId IS NOT NULL THEN 'School'
        END AS EducationOrganizationType,
        COALESCE(sea.StateEducationAgencyId, esc.StateEducationAgencyId, lea.StateEducationAgencyId, lea_sch.StateEducationAgencyId) AS StateEducationAgencyId, 
        COALESCE(esc.EducationServiceCenterId, lea.EducationServiceCenterId, lea_sch.EducationServiceCenterId) AS EducationServiceCenterId,
        COALESCE(lea.LocalEducationAgencyId, sch.LocalEducationAgencyId) AS LocalEducationAgencyId, 
        sch.SchoolId
FROM	edfi.EducationOrganization edorg
        LEFT JOIN edfi.StateEducationAgency sea
            ON edorg.EducationOrganizationId = sea.StateEducationAgencyId
        LEFT JOIN edfi.EducationServiceCenter esc
            ON edorg.EducationOrganizationId = esc.EducationServiceCenterId
        LEFT JOIN edfi.LocalEducationAgency lea
            ON edorg.EducationOrganizationId = lea.LocalEducationAgencyId
        LEFT JOIN edfi.School sch
            ON edorg.EducationOrganizationId = sch.SchoolId
        LEFT JOIN edfi.LocalEducationAgency lea_sch
            ON sch.LocalEducationAgencyId = lea_sch.LocalEducationAgencyId
WHERE	--Use same CASE as above to eliminate non-institutions (e.g. Networks)
        CASE 
            WHEN sea.StateEducationAgencyId IS NOT NULL THEN 'StateEducationAgency'
            WHEN esc.EducationServiceCenterId IS NOT NULL THEN 'EducationServiceCenter'
            WHEN lea.LocalEducationAgencyId IS NOT NULL THEN 'LocalEducationAgency'
            WHEN sch.SchoolId IS NOT NULL THEN 'School'
        END IS NOT NULL
ORDER BY edorg.EducationOrganizationId
";
                var cmd = new SqlCommand(sql, conn);

                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    int educationOrganizationIdCol = reader.GetOrdinal("EducationOrganizationId");
                    int educationOrganizationTypeCol = reader.GetOrdinal("EducationOrganizationType");
                    int stateEducationAgencyIdCol = reader.GetOrdinal("StateEducationAgencyId");
                    int educationServiceCenterIdCol = reader.GetOrdinal("EducationServiceCenterId");
                    int localEducationAgencyIdCol = reader.GetOrdinal("LocalEducationAgencyId");
                    int schoolIdCol = reader.GetOrdinal("SchoolId");

                    var list = new List<EducationOrganizationIdentifiers>();

                    while (reader.Read())
                    {
                        int educationOrganizationId = reader.GetInt32(educationOrganizationIdCol);
                        string educationOrganizationType = reader.GetString(educationOrganizationTypeCol);

                        int? stateEducationAgencyId =
                            reader.IsDBNull(stateEducationAgencyIdCol) ?
                                null as int? : reader.GetInt32(stateEducationAgencyIdCol);

                        int? educationServiceCenterId =
                            reader.IsDBNull(educationServiceCenterIdCol) ?
                                null as int? : reader.GetInt32(educationServiceCenterIdCol);

                        int? localEducationAgencyId =
                            reader.IsDBNull(localEducationAgencyIdCol) ?
                                null as int? : reader.GetInt32(localEducationAgencyIdCol);

                        int? schoolId =
                            reader.IsDBNull(schoolIdCol) ?
                                null as int? : reader.GetInt32(schoolIdCol);

                        list.Add(new EducationOrganizationIdentifiers(
                            educationOrganizationId, educationOrganizationType,
                            stateEducationAgencyId, educationServiceCenterId,
                            localEducationAgencyId, schoolId));
                    }

                    return list;
                }
            }
        }
    }
}