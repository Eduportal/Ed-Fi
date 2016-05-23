namespace EdFi.Common.Identity
{
    using System;

    public class Identity : IIdentity
    {
        public string UniqueId { get; set; }
        public Gender? BirthGender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string FamilyNames { get; set; }
        public string GivenNames { get; set; }
        public double Weight { get; set; }
        public bool IsMatch { get; set; }
        public IIdentifier[] Identifiers { get; set; }
    }
}