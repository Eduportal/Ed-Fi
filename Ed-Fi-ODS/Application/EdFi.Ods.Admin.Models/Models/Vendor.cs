using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdFi.Ods.Admin.Models
{
    public class Vendor
    {
        public Vendor()
        {
            Applications = new Collection<Application>();
            Users = new Collection<User>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VendorId { get; set; }
        public string VendorName { get; set; }

        [StringLength(255)]
        public string NamespacePrefix { get; set; }

        public virtual ICollection<Application> Applications { get; set; }
        public virtual ICollection<User> Users { get; set; }

        public Application CreateApplication(string applicationName)
        {
            var application = new Application { ApplicationName = applicationName, Vendor = this, ClaimSetName = "SIS Vendor" };
            Applications.Add(application);
            return application;
        }
    }
}