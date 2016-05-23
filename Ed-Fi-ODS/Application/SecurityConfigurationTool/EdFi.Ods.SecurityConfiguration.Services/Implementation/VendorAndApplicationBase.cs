using System.Linq;
using EdFi.Ods.Admin.Models;

namespace EdFi.Ods.SecurityConfiguration.Services.Implementation
{
    public abstract class VendorAndApplicationBase
    {
        protected static void RemoveApplicationHierarchy(IUsersContext context, Application app)
        {
            foreach (var client in app.ApiClients.ToList())
            {
                app.ApiClients.Remove(client);
            }

            foreach (var a in app.ApplicationEducationOrganizations.ToList())
            {
                app.ApplicationEducationOrganizations.Remove(a);
            }

            foreach (var profile in app.Profiles.ToList())
            {
                app.Profiles.Remove(profile);
            }

            context.Applications.Remove(app);
        }
    }
}