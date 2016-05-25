using EdFi.Common.SchoolIdentity;
using EdFi.Ods.Api.Models.Resources;
using EdFi.Ods.Api.Models.Resources.School;
using EdFi.Ods.Api.Models.Resources.Staff;

namespace EdFi.Ods.Api.Services.Extensions
{
    public static class ResourceExtensions
    {
        public static SchoolIdentityResource ToResource(this ISchoolIdentity schoolIdentity)
        {
            return new SchoolIdentityResource
            {
                EducationOrganizationId = schoolIdentity.EducationOrganizationId
            };
        }

        public static SchoolIdentityResource ToResource(this School school)
        {
            return new SchoolIdentityResource
            {
                EducationOrganizationId = school.SchoolId,
                StateOrganizationId = school.StateOrganizationId,
                NameOfInstitution = school.NameOfInstitution
            };
        }

        //public static StaffIdentityResource ToResource(this Staff staff)
        //{
        //    return new StaffIdentityResource
        //    {
        //        StaffUniqueId = staff.StaffUniqueId,
        //        FirstName = staff.FirstName,
        //        LastSurname = staff.LastSurname
        //    };
        //}
    }
}
