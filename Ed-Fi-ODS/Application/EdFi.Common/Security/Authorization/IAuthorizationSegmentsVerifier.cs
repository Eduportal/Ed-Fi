namespace EdFi.Common.Security.Authorization
{
    /// <summary>
    /// Defines a method for executing authorization segments with concrete values are present 
    /// in the ODS database as the final step of authorization of a single-item request.
    /// </summary>
    public interface IAuthorizationSegmentsVerifier
    {
        /// <summary>
        /// Verifies the specified segments exist in the ODS data.
        /// </summary>
        /// <param name="authorizationSegments">The authorization segments to be verified.</param>
        void Verify(AuthorizationSegmentCollection authorizationSegments);
    }
}