namespace EdFi.Identity.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EdFi.Common.Identity;

    public interface IIdentityMapper
    {
        IdentityResource MapToResource(IIdentity identity);
        EdFi.Common.Identity.IIdentity MapToModel(IdentityResource identity);
    }

    public class IdentityMapper : IIdentityMapper
    {
        public IdentityResource MapToResource(IIdentity identity)
        {
            var schoolAssociation = new SchoolAssociationResource();
            if (identity.Identifiers != null)
            {
                var lea = identity.Identifiers.FirstOrDefault(x => x.IdentifierTypeId == (int)IdentifierType.StudentSchoolLea);
                if (lea != null) schoolAssociation.LocalEducationAgencyName = lea.Value;
                var school = identity.Identifiers.FirstOrDefault(x => x.IdentifierTypeId == (int)IdentifierType.StudentSchoolName);
                if (school != null) schoolAssociation.SchoolName = school.Value;
                var year = identity.Identifiers.FirstOrDefault(x => x.IdentifierTypeId == (int)IdentifierType.StudentSchoolYear);
                if (year != null) schoolAssociation.SchoolYear = year.Value;
            }
            return new IdentityResource
            {
                UniqueId = identity.UniqueId,
                BirthGender = identity.BirthGender.ToString(),
                BirthDate = identity.BirthDate,
                FamilyNames = identity.FamilyNames,
                GivenNames = identity.GivenNames,
                Weight = identity.Weight,
                SchoolAssociation = schoolAssociation
            };
        }

        public EdFi.Common.Identity.IIdentity MapToModel(IdentityResource identity)
        {
            var id = new EdFi.Common.Identity.Identity
            {
                UniqueId = identity.UniqueId,
                BirthGender =
                    string.IsNullOrWhiteSpace(identity.BirthGender)
                        ? null
                        : (Gender?)Enum.Parse(typeof(Gender), identity.BirthGender, true),
                BirthDate = identity.BirthDate,
                FamilyNames = identity.FamilyNames,
                GivenNames = identity.GivenNames,
                Weight = identity.Weight
            };
            var identifiers = new List<IIdentifier>();
            if (identity.SchoolAssociation != null)
            {
                identifiers.Add(new Identifier()
                {
                    IdentifierTypeId = (int)IdentifierType.StudentSchoolLea,
                    Value = identity.SchoolAssociation.LocalEducationAgencyName
                });
                identifiers.Add(new Identifier()
                {
                    IdentifierTypeId = (int)IdentifierType.StudentSchoolName,
                    Value = identity.SchoolAssociation.SchoolName
                });
                identifiers.Add(new Identifier()
                {
                    IdentifierTypeId = (int)IdentifierType.StudentSchoolYear,
                    Value = identity.SchoolAssociation.SchoolYear
                });
            }
            id.Identifiers = identifiers.ToArray();
            return id;
        }
    }
}