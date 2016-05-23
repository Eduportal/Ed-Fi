using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Common.Extensions;

namespace EdFi.Common.Security.Authorization
{
    /// <summary>
    /// Represents an authorization segment involving claims on one side and an endpoint that must be related to at least one of the claims.
    /// </summary>
    public class ClaimsAuthorizationSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimsAuthorizationSegment"/> class using the supplied claim values and target endpoint.
        /// </summary>
        /// <param name="claimNamesAndValues">The claim names and values, represented as a collection of tuples.</param>
        /// <param name="targetEndpoint"></param>
        public ClaimsAuthorizationSegment(IEnumerable<Tuple<string, object>> claimNamesAndValues, AuthorizationSegmentEndpoint targetEndpoint)
        {
            ClaimsEndpoints = claimNamesAndValues
                .Select(cv => 
                    new AuthorizationSegmentEndpointWithValue(
                        cv.Item1, cv.Item2.GetType(), cv.Item2))
                .ToList()
                .AsReadOnly();

            TargetEndpoint = targetEndpoint;
        }

        /// <summary>
        /// Gets the collection of values, one of which must be associated with the <see cref="TargetEndpoint"/>.
        /// </summary>
        public IReadOnlyList<AuthorizationSegmentEndpointWithValue> ClaimsEndpoints { get; private set; }

        /// <summary>
        /// Gets the target endpoint for the segment.
        /// </summary>
        public AuthorizationSegmentEndpoint TargetEndpoint { get; private set; }

        /// <summary>
        /// Returns a text representation of the claims authorization segment.
        /// </summary>
        /// <returns>A representation of the claims authorization segment.</returns>
        public override string ToString()
        {
            return string.Format("Claims: {0} to {1}",
                string.Join(", ", ClaimsEndpoints.Select(x => x.ToString())),
                TargetEndpoint);
        }
    }
}