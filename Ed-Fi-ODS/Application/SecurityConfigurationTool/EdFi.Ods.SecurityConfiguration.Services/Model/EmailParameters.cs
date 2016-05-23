using System.Collections.Generic;

namespace EdFi.Ods.SecurityConfiguration.Services.Model
{
    public class EmailParameters
    {
        public string RecipientEmailAddress { get; set; }
        public string ChallengeId { get; set; }
        public string ApplicationName { get; set; }
        public IEnumerable<string> EducationOrganization { get; set; }
    }
}