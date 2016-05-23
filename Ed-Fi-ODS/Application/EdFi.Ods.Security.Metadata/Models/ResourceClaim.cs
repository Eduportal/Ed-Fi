using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdFi.Ods.Security.Metadata.Models
{
    public class ResourceClaim
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResourceClaimId { get; set; }

        [StringLength(255)]
        [Required]
        public string DisplayName { get; set; }

        /// <summary>
        /// ResourceName is actually an Uri so length needs to be around 2048
        /// </summary>
        [StringLength(2048)]
        [Required]
        public string ResourceName { get; set; }

        /// <summary>
        /// ClaimName is actually an Uri so length needs to be around 2048
        /// </summary>
        [StringLength(2048)]
        [Required]
        public string ClaimName { get; set; }

        [Required]
        public Application Application { get; set; }

        public int? ParentResourceClaimId { get; set; }
        public virtual ResourceClaim ParentResourceClaim { get; set; }


        public static string GetResourceName(string resourceUri)
        {
            var pos = resourceUri.LastIndexOf('/');

            if (pos >= 0)
                return resourceUri.Substring(pos + 1);

            return resourceUri;
        }
    }
}
