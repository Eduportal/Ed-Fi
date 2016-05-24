using EdFi.Common.SchoolIdentity;
using EdFi.Ods.Api.Models.Resources;

namespace EdFi.Ods.Api.Data.Mappings
{
    public static class SchoolIdentityExtensions
    {
        public static SchoolIdentityResource ToResource(this ISchoolIdentity schoolIdentity)
        {
            return new SchoolIdentityResource
            {
                EducationOrganizationId = schoolIdentity.EducationOrganizationId
            };
        }
    }
}
