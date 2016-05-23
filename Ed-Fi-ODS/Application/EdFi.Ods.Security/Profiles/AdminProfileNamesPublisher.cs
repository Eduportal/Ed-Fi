using System;
using System.Linq;
using System.Threading.Tasks;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.Api.Common;
using log4net;

namespace EdFi.Ods.Security.Profiles
{
    public class AdminProfileNamesPublisher : IAdminProfileNamesPublisher
    {
        private readonly UsersContext _usersContext;
        private readonly IProfileResourcesProvider _profileResourcesProvider;
        private readonly ILog _logger = LogManager.GetLogger(typeof(AdminProfileNamesPublisher));

        public AdminProfileNamesPublisher(UsersContext usersContext, IProfileResourcesProvider profileResourcesProvider)
        {
            _usersContext = usersContext;
            _profileResourcesProvider = profileResourcesProvider;
        }

        public async Task<bool> PublishProfilesAsync()
        {
             return await Task.Factory.StartNew(() => PublishProfiles());
        }

        private bool PublishProfiles()
        {
            try
            {
                //get the set of profiles from any Profiles.xml files found in assemblies
                var definedProfiles =
                   _profileResourcesProvider.GetProfileResources()
                   .Select(x => x.ProfileName).Distinct();

                //determine which Profiles from the Profiles.xml files do not exist in the admin database
                var publishedProfiles = _usersContext.Profiles
                    .Select(x => x.ProfileName)
                    .ToList();

                var profilesToInsert = definedProfiles
                    .Except(publishedProfiles)
                    .ToList();

                //if there are none to insert return
                if (!profilesToInsert.Any())
                    return true;

                //for each profile not in the database, add it
                foreach (var profileName in profilesToInsert)
                {
                    _usersContext.Profiles.Add(new Profile { ProfileName = profileName });
                }

                _usersContext.SaveChanges();

                return true;
            }
            catch (Exception exception)
            {
                //If an exception occurs log it and return false since it is an async call.
                _logger.Error("An error occured when attempting to publish Profiles to the admin database.", exception);
                return false;
            }
        }
    }
}
