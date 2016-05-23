using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdFi.Ods.Admin.Models
{
    public class Profile
    {
        public Profile()
        {
            Applications = new Collection<Application>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProfileId { get; set; }
        
        [Required]
        public string ProfileName { get; set; }

        public virtual ICollection<Application> Applications { get; set; }
    }
}
