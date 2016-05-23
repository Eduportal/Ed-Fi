using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdFi.Ods.Security.Metadata.Models
{
    public class ClaimSet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClaimSetId { get; set; }

        [StringLength(255)]
        [Required]
        public string ClaimSetName { get; set; }

        [Required]
        public Application Application { get; set; }
    }
}
