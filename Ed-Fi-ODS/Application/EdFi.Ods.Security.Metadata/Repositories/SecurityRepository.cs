using EdFi.Ods.Security.Metadata.Contexts;
using System;
using System.Data.Entity;
using System.Linq;

namespace EdFi.Ods.Security.Metadata.Repositories
{
    public class SecurityRepository : SecurityRepositoryBase, ISecurityRepository
    {
        public SecurityRepository(SecurityContext context)
        {
            var application = context.Applications.First(app => app.ApplicationName.Equals("Ed-Fi ODS API", StringComparison.InvariantCultureIgnoreCase));

            var actions = context.Actions.ToList();

            var claimSets = context.ClaimSets.Include(cs => cs.Application).ToList();

            var resourceClaims = context.ResourceClaims.Include(rc => rc.Application)
                                                    .Include(rc => rc.ParentResourceClaim)
                                                    .Where(rc => rc.Application.ApplicationId.Equals(application.ApplicationId))
                                                    .ToList();

            var authorizationStrategies = context.AuthorizationStrategies.Include(auth => auth.Application)
                                                                      .Where(auth => auth.Application.ApplicationId.Equals(application.ApplicationId))
                                                                      .ToList();

            var claimSetResourceClaims = context.ClaimSetResourceClaims.Include(csrc => csrc.Action)
                    .Include(csrc => csrc.ClaimSet)
                    .Include(csrc => csrc.ResourceClaim)
                    .Where(csrc => csrc.ResourceClaim.Application.ApplicationId.Equals(application.ApplicationId))
                    .ToList();

            var resourceClaimAuthorizationStrategies = context.ResourceClaimAuthorizationStrategies.Include(rcas => rcas.Action)
                                                                                                .Include(rcas => rcas.AuthorizationStrategy)
                                                                                                .Include(rcas => rcas.ResourceClaim)
                                                                                                .Where(rcas => rcas.ResourceClaim.Application.ApplicationId.Equals(application.ApplicationId))
                                                                                                .ToList();

            Intitalize(application, actions, claimSets, resourceClaims, authorizationStrategies, claimSetResourceClaims, resourceClaimAuthorizationStrategies);
        }
    }
}
