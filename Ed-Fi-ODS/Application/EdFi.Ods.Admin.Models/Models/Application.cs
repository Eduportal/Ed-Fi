using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdFi.Ods.Admin.Models
{
    public class Application
    {
        public Application()
        {
            ApplicationEducationOrganizations = new Collection<ApplicationEducationOrganization>();
            ApiClients = new Collection<ApiClient>();
            Profiles = new Collection<Profile>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        [StringLength(255)]
        public string ClaimSetName { get; set; }
        public virtual Vendor Vendor { get; set; }
        public virtual ICollection<ApplicationEducationOrganization> ApplicationEducationOrganizations { get; set; }
        public virtual ICollection<ApiClient> ApiClients { get; set; }
        public virtual ICollection<Profile> Profiles { get; set; }

        public ApplicationEducationOrganization CreateEducationOrganizationAssociation(int educationOrganizationId)
        {
            var educationOrganization = new ApplicationEducationOrganization { EducationOrganizationId = educationOrganizationId, Application = this};
            ApplicationEducationOrganizations.Add(educationOrganization);
            return educationOrganization;
        }
    }
}