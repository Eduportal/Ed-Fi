using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdFi.Ods.Security.Metadata.Models
{
    public class AuthorizationStrategy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuthorizationStrategyId { get; set; }

        [StringLength(255)]
        [Required]
        public string DisplayName { get; set; }

        [StringLength(255)]
        [Required]
        public string AuthorizationStrategyName { get; set; }

        [Required]
        public Application Application { get; set; }
    }
}
