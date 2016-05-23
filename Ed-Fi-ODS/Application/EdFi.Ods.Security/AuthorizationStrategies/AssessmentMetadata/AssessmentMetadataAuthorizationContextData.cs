namespace EdFi.Ods.Security.AuthorizationStrategies.AssessmentMetadata
{
    /// <summary>
    /// Contains the pertinent Ed-Fi data necessary for making authorization decisions.
    /// </summary>
    public class AssessmentMetadataAuthorizationContextData
    {
        // Ownership
        public string Namespace { get; set; }

        //Assessment Metadata
        public string AssessmentFamilyTitle { get; set; }
        public string AssessmentTitle { get; set; }
    }
}