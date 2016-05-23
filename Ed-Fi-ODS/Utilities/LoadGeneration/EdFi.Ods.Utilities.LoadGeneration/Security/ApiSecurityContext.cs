namespace EdFi.Ods.Utilities.LoadGeneration.Security
{
    public class ApiSecurityContext
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string ApiUrl { get; set; }
        public string OAuthUrl { get; set; }
        public int[] LocalEducationAgencyIds { get; set; }
        public string ApiSdkAssemblyPath { get; set; }
        public int SchoolYear { get; set; }
        public bool RefreshCache { get; set; }
    }
}