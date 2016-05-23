namespace EdFi.Identity.Models
{
    using System;

    public class IdentityResource  
    {
        public string UniqueId { get; set; }

        public string BirthGender { get; set; }

        public DateTime? BirthDate { get; set; }

        public string FamilyNames { get; set; }

        public string GivenNames { get; set; }

        public double Weight { get; set; }

        public SchoolAssociationResource SchoolAssociation { get; set; }
    }

    public class SchoolAssociationResource
    {
        public string SchoolName { get; set; }
        public string LocalEducationAgencyName { get; set; }
        public string SchoolYear { get; set; }
    }
}
