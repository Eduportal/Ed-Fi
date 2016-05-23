using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdFi.Ods.Security.Metadata.Models
{
    public class ClaimSetResourceClaim
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClaimSetResourceClaimId { get; set; }

        public Action Action { get; set; }

        public ClaimSet ClaimSet { get; set; }

        public ResourceClaim ResourceClaim { get; set; }
    }
}
