using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdFi.Ods.Security.Metadata.Models
{
    public class ResourceClaimAuthorizationStrategy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResourceClaimAuthorizationStrategyId { get; set; }

        public Action Action { get; set; }

        public AuthorizationStrategy AuthorizationStrategy { get; set; }

        public ResourceClaim ResourceClaim { get; set; }
        
        [StringLength(255)]
        public string Scheme { get; set; }
    }
}
