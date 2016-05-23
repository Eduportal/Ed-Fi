using System.Collections.Generic;
using EdFi.Ods.SecurityConfiguration.Services.Model;

namespace EdFi.Ods.SecurityConfiguration.Services
{
    public interface IProfileListService
    {
        IEnumerable<Profile> GetAllProfiles();
    }
}