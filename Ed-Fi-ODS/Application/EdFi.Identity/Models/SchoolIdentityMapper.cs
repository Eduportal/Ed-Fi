
using EdFi.Common.SchoolIdentity;

namespace EdFi.Identity.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EdFi.Common.Identity;

    public interface ISchoolIdentityMapper
    {
        SchoolIdentityResource MapToResource(ISchoolIdentity schoolIdentity);
    }

    public class SchoolIdentityMapper : ISchoolIdentityMapper
    {
        public SchoolIdentityResource MapToResource(ISchoolIdentity schoolIdentity)
        {
            return new SchoolIdentityResource
            {
                EducationOrganizationId = schoolIdentity.EducationOrganizationId
            };
        }
    }
}