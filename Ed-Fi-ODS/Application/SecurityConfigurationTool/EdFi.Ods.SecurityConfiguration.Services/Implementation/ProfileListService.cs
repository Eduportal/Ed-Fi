using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.SecurityConfiguration.Services.Model;
using AdminContext = EdFi.Ods.Admin.Models.UsersContext;

namespace EdFi.Ods.SecurityConfiguration.Services.Implementation
{
    public class ProfileListService : IProfileListService
    {
        public IEnumerable<Profile> GetAllProfiles()
        {
            using (var context = new AdminContext())
            {
                return context.Profiles.Select(p => new Profile
                {
                    ProfileId = p.ProfileId,
                    ProfileName = p.ProfileName,
                }).ToList();
            }
        }
    }
}
