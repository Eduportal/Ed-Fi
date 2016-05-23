using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdFi.Ods.Security.Metadata.Models
{
    public class Action
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ActionId { get; set; }

        [StringLength(255)]
        [Required]
        public string ActionName { get; set; }

        [StringLength(2048)]
        [Required]
        public string ActionUri { get; set; }
    }
}
