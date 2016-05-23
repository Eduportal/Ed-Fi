using System.Collections.Generic;
using EdFi.Ods.SecurityConfiguration.Services.Model;

namespace EdFi.Ods.SecurityConfiguration.Services
{
    public interface IEducationOrganizationProvider
    {
        IEnumerable<EducationOrganization> GetAll();
        IEnumerable<EducationOrganization> GetLocalEducationAgencies();
        IEnumerable<EducationOrganization> GetSchools();
    }
}