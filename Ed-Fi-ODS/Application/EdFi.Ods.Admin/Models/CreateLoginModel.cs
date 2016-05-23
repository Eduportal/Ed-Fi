using System.ComponentModel.DataAnnotations;

namespace EdFi.Ods.Admin.Models
{
    public class CreateLoginModel
    {
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string Name { get; set; }
    }
}