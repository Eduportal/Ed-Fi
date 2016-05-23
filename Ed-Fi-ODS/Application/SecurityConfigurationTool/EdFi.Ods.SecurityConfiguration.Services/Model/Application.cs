using System.Collections.Generic;

namespace EdFi.Ods.SecurityConfiguration.Services.Model
{
    public class Application
    {
        public int ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string ClaimSetName { get; set; }
        public IEnumerable<EducationOrganization> EducationOrganizations { get; set; }
        public string KeyStatus { get; set; }
        public IEnumerable<Profile> AssociatedProfiles { get; set; }
        public string ActivationCode { get; set; }
    }
}