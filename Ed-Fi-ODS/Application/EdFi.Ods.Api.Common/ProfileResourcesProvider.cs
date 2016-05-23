using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using EdFi.Ods.Api.Common.Filters;
using log4net;

namespace EdFi.Ods.Api.Common
{
    public class ProfileResource
    {
        public string ProfileName { get; set; }
        public string Resource { get; set; }
    }

    public interface IProfileResourcesProvider
    {
        List<ProfileResource> GetProfileResources();
    }

    public class ProfileResourcesProvider : IProfileResourcesProvider
    {
        private readonly Lazy<List<ProfileResource>> _profileResources = new Lazy<List<ProfileResource>>(GetAllProfileResourcesInAppDomain, true);
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProfilesAuthorizationFilter));

        public List<ProfileResource> GetProfileResources()
        {
            return _profileResources.Value;
        }

        private static List<ProfileResource> GetAllProfileResourcesInAppDomain()
        {
            var returnProfileResourceList = new List<ProfileResource>();

            //Get assemblies that are profile assemblies
            var assemblies = from a in AppDomain.CurrentDomain.GetAssemblies()
                             where a.FullName.Contains("Profiles") // TODO: Embedded convention
                             select a;

            //pull profiles from each
            foreach (var resources in assemblies.Select(GetProfilesResourcesFromAssemblyXml))
            {
                returnProfileResourceList.AddRange(resources);
            }

            return returnProfileResourceList;
        }

        private static IEnumerable<ProfileResource> GetProfilesResourcesFromAssemblyXml(Assembly assembly)
        {
            var resources = new List<ProfileResource>();

            //check if there is a Profiles.xml file in the assembly
            if (!assembly.GetManifestResourceNames().Any(x => x.EndsWith("Profiles.xml"))) // TODO: Embedded convention
            {
                _logger.Warn(
                    string.Format("A Profiles assembly named {0} was found but no 'Profiles.xml' file was found in it.",
                        assembly.FullName));
                return resources;
            }

            //get the Profiles.xml file's full resource name
            var resourceName = assembly.GetManifestResourceNames().First(x => x.EndsWith("Profiles.xml")); // TODO: Embedded convention

            //Get Profile to REsource information from the file
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) return resources;
                using (var reader = new StreamReader(stream))
                {
                    var profilesXDoc = XDocument.Load(reader);
                    var profiles = from p in profilesXDoc.Descendants("Profile")
                                   select p;
                    foreach (var profile in profiles)
                    {
                        var profileName = (string)profile.Attribute("name");

                        resources.AddRange((from r in profile.Descendants("Resource")
                                            select new ProfileResource
                                            {
                                                ProfileName = profileName,
                                                Resource = (string)r.Attribute("name")
                                            }).ToList());
                    }
                }
            }
            return resources;
        }
    }
}
