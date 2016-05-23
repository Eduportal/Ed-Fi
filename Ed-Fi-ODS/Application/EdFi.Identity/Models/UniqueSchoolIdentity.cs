using System;
using EdFi.Common.SchoolIdentity;

namespace EdFi.Identity.Models
{
    public class UniqueSchoolIdentity : IUniqueSchoolIdentity
    {
        public ISchoolIdentity Get(string uniqueId)
        {
            return new SchoolIdentity
            {
                EducationOrganizationId = 12345
            };
        }

        public ISchoolIdentity[] Get(ISchoolIdentity identity)
        {
            return new ISchoolIdentity[] { identity };
        }

        public ISchoolIdentity Post(ISchoolIdentity command)
        {
            command.EducationOrganizationId = 12345;
            return command;
        }
    }
}