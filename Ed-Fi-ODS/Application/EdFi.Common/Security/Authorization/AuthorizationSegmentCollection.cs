using System.Collections.Generic;
using System.Linq;
using EdFi.Common.Extensions;

namespace EdFi.Common.Security.Authorization
{
    /// <summary>
    /// Represents a collection of authorization segments, separated by types (claims-based associations 
    /// and existing values associations).
    /// </summary>
    public class AuthorizationSegmentCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationSegmentCollection"/> class with the 
        /// supplied authorization segments.
        /// </summary>
        /// <param name="claimsAuthorizationSegments">A collection of segments indicating relationships required of the caller's claims.</param>
        /// <param name="existingValuesAuthorizationSegments">A collection of segments indicating related data that must already exist.</param>
        public AuthorizationSegmentCollection(
            IEnumerable<ClaimsAuthorizationSegment> claimsAuthorizationSegments,
            IEnumerable<ExistingValuesAuthorizationSegment> existingValuesAuthorizationSegments)
        {
            ClaimsAuthorizationSegments = claimsAuthorizationSegments.ToList().AsReadOnly();
            ExistingValuesAuthorizationSegments = existingValuesAuthorizationSegments.ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the claims authorization segments.
        /// </summary>
        public IReadOnlyList<ClaimsAuthorizationSegment> ClaimsAuthorizationSegments { get; private set; }

        /// <summary>
        /// Gets the existing values authorization segments.
        /// </summary>
        public IReadOnlyList<ExistingValuesAuthorizationSegment> ExistingValuesAuthorizationSegments { get; private set; }

        /// <summary>
        /// Gets the empty <see cref="AuthorizationSegmentCollection"/>.
        /// </summary>
        public static readonly AuthorizationSegmentCollection Empty =
            new AuthorizationSegmentCollection(
                new List<ClaimsAuthorizationSegment>(),
                new List<ExistingValuesAuthorizationSegment>());
    }
}