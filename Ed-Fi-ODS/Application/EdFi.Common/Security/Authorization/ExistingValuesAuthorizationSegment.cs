using System.Collections.Generic;
using System.Linq;

namespace EdFi.Common.Security.Authorization
{
    /// <summary>
    /// Represents an authorization segment involving two arbitrary endpoints that must be related to eachother.
    /// </summary>
    public class ExistingValuesAuthorizationSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExistingValuesAuthorizationSegment"/> class using the supplied endpoints.
        /// </summary>
        /// <param name="endpoint1">One of the endpoints of the segment.</param>
        /// <param name="endpoint2">Another endpoint in the segment.</param>
        public ExistingValuesAuthorizationSegment(AuthorizationSegmentEndpoint endpoint1, AuthorizationSegmentEndpoint endpoint2)
        {
            Endpoints = (new List<AuthorizationSegmentEndpoint> {endpoint1, endpoint2})
                .AsReadOnly();
        }

        /// <summary>
        /// Gets the endpoints for the authorization segment.
        /// </summary>
        public IReadOnlyList<AuthorizationSegmentEndpoint> Endpoints { get; private set; }

        /// <summary>
        /// Returns a text representation of the existing values segment.
        /// </summary>
        /// <returns>A representation of the existing values segment.</returns>
        public override string ToString()
        {
            return string.Format(
                "Existing Values: {0}",
                string.Join(" and ", Endpoints.Select(x => x.ToString())));
        }
    }
}