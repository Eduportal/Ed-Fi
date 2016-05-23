using EdFi.Common.Configuration;
using EdFi.Common.Database;
using EdFi.Ods.Api.Common.Authorization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using EdFi.Ods.Common.Utils.Extensions;

namespace EdFi.Ods.Api.Services.Authorization
{
    /// <summary>
    /// Implements a token validator that retrieves the API client's details from the Admin database.
    /// </summary>
    public class OAuthTokenValidator : IOAuthTokenValidator
    {
        private readonly Lazy<string> _connectionString;
        private readonly Lazy<int> _bearerTokenTimeoutMinutes;

        private const string ConfigBearerTokenTimeoutMinutes = "BearerTokenTimeoutMinutes";

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthTokenValidator"/> class.
        /// </summary>
        /// <param name="adminDatabaseConnectionStringProvider">The connection string provider.</param>
        /// <param name="configValueProvider">The configuration value provider.</param>
        public OAuthTokenValidator(
            IDatabaseConnectionStringProvider adminDatabaseConnectionStringProvider,
            IConfigValueProvider configValueProvider)
        {
            // Lazy initialization
            _connectionString = new Lazy<string>(adminDatabaseConnectionStringProvider.GetConnectionString);

            _bearerTokenTimeoutMinutes = new Lazy<int>(() =>
                Convert.ToInt32(configValueProvider.GetValue(ConfigBearerTokenTimeoutMinutes) ?? "30"));
        }

        /// <summary>
        /// Loads the API client details for the supplied token from the Admin database.
        /// </summary>
        /// <param name="token">The OAuth security token for which API client details should be retrieved.</param>
        /// <returns>A populated <see cref="ApiClientDetails"/> instance.</returns>
        public ApiClientDetails GetClientDetailsForToken(Guid token)
        {
            const string accessTokenParameterName = "AccessToken";
            const string timeoutMinutesParameterName = "TimeoutMinutes";
            
            using (var connection = new SqlConnection(_connectionString.Value))
            {
                connection.Open();
                
                // TODO: DLP - Need to modify DB and stored procedure to allow configuration of EdOrgs, and specifying the ed org type
                string cmdText = String.Format("AccessTokenIsValid @{0}, @{1}", 
                    accessTokenParameterName, 
                    timeoutMinutesParameterName);

                var cmd = new SqlCommand(cmdText, connection);

                var tokenParameter = new SqlParameter("@" + accessTokenParameterName, token);
                cmd.Parameters.Add(tokenParameter);

                var timeoutParameter = new SqlParameter("@" + timeoutMinutesParameterName, _bearerTokenTimeoutMinutes.Value);
                cmd.Parameters.Add(timeoutParameter);

                var apiClientDetails = new ApiClientDetails();

                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    int apiKeyColumn = reader.GetOrdinal("Key");
                    
                    int educationOrganizationIdColumn = reader.GetOrdinal("EducationOrganizationId");
                    int profileNameColumn = reader.GetOrdinal("ProfileName");
                    int claimSetNameColumn = reader.GetOrdinal("ClaimSetName");
                    int namespacePrefixColumn = reader.GetOrdinal("NamespacePrefix");

                    if (reader.Read())
                    {
                        apiClientDetails.ApiKey = reader.GetString(apiKeyColumn);
                        apiClientDetails.ClaimSetName = reader.GetString(claimSetNameColumn);
                        apiClientDetails.NamespacePrefix = reader.GetString(namespacePrefixColumn);

                        var educationOrganizationIds = new HashSet<int>();
                        var profileNames = new HashSet<string>();

                        do
                        {
                            int educationOrganizationId;

                            if (TryGetInt(reader, educationOrganizationIdColumn, out educationOrganizationId))
                                educationOrganizationIds.Add(educationOrganizationId);

                            string profileName;

                            if (TryGetString(reader, profileNameColumn, out profileName)
                                && !string.IsNullOrEmpty(profileName))
                            {
                                profileNames.Add(profileName);
                            }
                        } while (reader.Read());
                        
                        // Add the distinct EdOrgIds and Profile names to their respective collections
                        educationOrganizationIds.ForEach(x => apiClientDetails.EducationOrganizationIds.Add(x));
                        profileNames.ForEach(x => apiClientDetails.Profiles.Add(x));
                    }
                }

                return apiClientDetails;
            }
        }

        private bool TryGetString(SqlDataReader reader, int ordinalPos, out string value)
        {
            value = null;

            if (reader.IsDBNull(ordinalPos))
                return false;

            value = reader.GetString(ordinalPos);
            return true;
        }

        private bool TryGetInt(SqlDataReader reader, int ordinalPos, out int value)
        {
            value = 0;

            if (reader.IsDBNull(ordinalPos))
                return false;

            value = reader.GetInt32(ordinalPos);
            return true;
        }
    }
}