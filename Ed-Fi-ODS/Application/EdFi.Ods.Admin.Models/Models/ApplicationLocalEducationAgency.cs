using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdFi.Ods.Admin.Models
{
    public sealed class ApplicationEducationOrganization
    {
        public ApplicationEducationOrganization()
        {
            Clients = new Collection<ApiClient>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApplicationEducationOrganizationId { get; set; }
        public Application Application { get; set; }
        public int EducationOrganizationId { get; set; }
        public ICollection<ApiClient> Clients { get; set; }
    }
}