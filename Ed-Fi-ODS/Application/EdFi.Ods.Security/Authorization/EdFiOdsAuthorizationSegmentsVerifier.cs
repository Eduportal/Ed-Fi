using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using EdFi.Common.Database;
using EdFi.Common.Extensions;
using EdFi.Common.Security;
using EdFi.Common.Security.Authorization;

namespace EdFi.Ods.Security.Authorization
{
    /// <summary>
    /// Executes authorization rules against an Ed-Fi ODS database as the final step of authorization.
    /// </summary>
    public class EdFiOdsAuthorizationSegmentsVerifier : IAuthorizationSegmentsVerifier
    {
        private readonly IDatabaseConnectionStringProvider _odsDatabaseConnectionStringProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdFiOdsAuthorizationSegmentsVerifier"/> class using
        /// the supplied connection string provider.
        /// </summary>
        /// <param name="odsDatabaseConnectionStringProvider">The connection string provider for obtaining a 
        /// connection string to the Ed-Fi ODS database.</param>
        public EdFiOdsAuthorizationSegmentsVerifier(IDatabaseConnectionStringProvider odsDatabaseConnectionStringProvider)
        {
            _odsDatabaseConnectionStringProvider = odsDatabaseConnectionStringProvider;
        }

        /// <summary>
        /// Verifies that the specified segments exist in the data, as the final step of authorization.
        /// </summary>
        /// <param name="authorizationSegments">The segments to be verified.</param>
        public void Verify(AuthorizationSegmentCollection authorizationSegments)
        {
            if (authorizationSegments == null)
                throw new ArgumentNullException("authorizationSegments");

            if (!authorizationSegments.ClaimsAuthorizationSegments.Any()
                && !authorizationSegments.ExistingValuesAuthorizationSegments.Any())
                throw new ArgumentException("No authorization segments have been defined.");

            using (var conn = new SqlConnection(_odsDatabaseConnectionStringProvider.GetConnectionString()))
            {
                var cmd = new SqlCommand();

                int parameterIndex = 0;
                string sql = BuildAuthorizationSql(authorizationSegments, cmd, ref parameterIndex);

                cmd.CommandText = sql;
                cmd.Connection = conn;
                conn.Open();

                var result = Convert.ToInt32(cmd.ExecuteScalar());

                if (result == 0)
                    throw new EdFiSecurityException("Authorization denied.  The claim does not have any established relationships with the requested resource.");

                conn.Close();
            }
        }

        private const string MainTemplate = @"
IF EXISTS (
{0}
    )
    SELECT 1
ELSE
    SELECT 0";

        private const string StatementTemplate = @"
        SELECT	*
        FROM	auth.{0}To{2} a
        WHERE	a.{0}{1}
            and a.{2}{3}";

        private static string BuildAuthorizationSql(AuthorizationSegmentCollection authorizationSegments, SqlCommand cmd, ref int parameterIndex)
        {
            var segmentStatements = new List<string>();  // TODO: If !rule.ChoiceValues.Any() continue;

            foreach (var authorizationSegment in authorizationSegments.ClaimsAuthorizationSegments)
            {
                // Within each claim segment, group the values by ed org type, and combine with "OR"
                var claimEndpointsGroupedByName =
                    from ep in authorizationSegment.ClaimsEndpoints
                    group ep by ep.Name
                    into g
                    select g;

                var segmentExpressions = new List<string>();

                foreach (var claimEndpointsWithSameName in claimEndpointsGroupedByName)
                {
                    string claimEndpointName = claimEndpointsWithSameName.Key;
                    string targetEndpointName = authorizationSegment.TargetEndpoint.Name;
                    var targetEndpointWithValue =
                        authorizationSegment.TargetEndpoint as AuthorizationSegmentEndpointWithValue;

                    // This should never happen
                    if (targetEndpointWithValue == null)
                        throw new Exception("The claims-based authorization segment target endpoint for a single-item authorization was not defined with a value.");

                    if (string.Compare(targetEndpointName, claimEndpointName,
                            StringComparison.InvariantCultureIgnoreCase) < 0)
                    {
                        segmentExpressions.Add(
                            string.Format(StatementTemplate,
                                targetEndpointName, GetSingleValueCriteriaExpression(targetEndpointWithValue, cmd, ref parameterIndex),
                                claimEndpointName, GetMultiValueCriteriaExpression(claimEndpointsWithSameName, cmd, ref parameterIndex))
                            );
                    }
                    else
                    {
                        segmentExpressions.Add(
                            string.Format(StatementTemplate,
                                claimEndpointName, GetMultiValueCriteriaExpression(claimEndpointsWithSameName, cmd, ref parameterIndex),
                                targetEndpointName, GetSingleValueCriteriaExpression(targetEndpointWithValue, cmd, ref parameterIndex))
                            );
                    }
                }

                // Combine multiple statements (resulting from multiple ed org types in claims) using "OR"
                segmentStatements.Add(string.Join("\r\n\t) OR EXISTS (\r\n\t", segmentExpressions).Parenthesize());
            }

            foreach (var authorizationSegment in authorizationSegments.ExistingValuesAuthorizationSegments)
            {
                // This should never happen
                if (authorizationSegment.Endpoints.Count != 2)
                    throw new Exception("The existing values authorization segment for a single-item authorization did not contain exactly two endpoints.");

                var endpoint1 = authorizationSegment.Endpoints.ElementAt(0) as AuthorizationSegmentEndpointWithValue;
                var endpoint2 = authorizationSegment.Endpoints.ElementAt(1) as AuthorizationSegmentEndpointWithValue;

                // This should never happen
                if (endpoint1 == null || endpoint2 == null)
                    throw new Exception("One or both of the existing values authorization segment endpoints for a single-item authorization was not defined with a value.");

                if (string.Compare(endpoint1.Name, endpoint2.Name,
                        StringComparison.InvariantCultureIgnoreCase) < 0)
                {
                    segmentStatements.Add(
                        string.Format(StatementTemplate,
                            endpoint1.Name, GetSingleValueCriteriaExpression(endpoint1, cmd, ref parameterIndex),
                            endpoint2.Name, GetSingleValueCriteriaExpression(endpoint2, cmd, ref parameterIndex))
                        );
                }
                else
                {
                    segmentStatements.Add(
                        string.Format(StatementTemplate,
                            endpoint2.Name, GetSingleValueCriteriaExpression(endpoint2, cmd, ref parameterIndex),
                            endpoint1.Name, GetSingleValueCriteriaExpression(endpoint1, cmd, ref parameterIndex))
                        );
                }
            }

            string statements = string.Join("\r\n\t) AND EXISTS (\r\n\t", segmentStatements);

            string sql = string.Format(MainTemplate, statements);

            return sql;
        }

        private static string GetSingleValueCriteriaExpression(AuthorizationSegmentEndpointWithValue segmentEndpoint, SqlCommand cmd, ref int parameterIndex)
        {
            string parameterName = "@p" + parameterIndex++;

            cmd.Parameters.AddWithValue(parameterName, segmentEndpoint.Value);

            return " = " + parameterName;
        }

        private static string GetMultiValueCriteriaExpression(IEnumerable<AuthorizationSegmentEndpointWithValue> endpointsWithValues, SqlCommand cmd, ref int parameterIndex)
        {
            var firstEndpointValue = endpointsWithValues.FirstOrDefault();

            if (firstEndpointValue == null)
                throw new Exception("There were no endpoint values from which to build an authorization query.");

            // If there's only 1 rule, use the single value method
            if (endpointsWithValues.Count() == 1)
                return GetSingleValueCriteriaExpression(firstEndpointValue, cmd, ref parameterIndex);

            var sb = new StringBuilder();

            foreach (var endpointWithValue in endpointsWithValues)
            {
                string parameterName = "@p" + parameterIndex++;

                if (sb.Length == 0)
                    sb.Append(parameterName);
                else
                    sb.Append(", " + parameterName);

                cmd.Parameters.AddWithValue(parameterName, endpointWithValue.Value);
            }

            return " IN (" + sb + ")";
        }
    }
}