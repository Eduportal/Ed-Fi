using System;

namespace EdFi.Common.Identity
{
    public interface IIdentity
    {
        string UniqueId { get; set; }
        Gender? BirthGender { get; set; }
        DateTime? BirthDate { get; set; }
        string FamilyNames { get; set; }
        string GivenNames { get; set; }
        double Weight { get; set; }
        bool IsMatch { get; set; }
        IIdentifier[] Identifiers { get; set; }
    }
}
