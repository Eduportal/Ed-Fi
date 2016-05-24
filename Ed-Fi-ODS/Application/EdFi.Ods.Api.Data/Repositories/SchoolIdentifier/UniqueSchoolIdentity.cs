using EdFi.Common.SchoolIdentity;

namespace EdFi.Ods.Api.Data.Repositories.SchoolIdentifier
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
    }
}