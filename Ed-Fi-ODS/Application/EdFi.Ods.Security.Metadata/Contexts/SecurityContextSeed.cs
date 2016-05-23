using System.Linq;
using EdFi.Ods.Security.Metadata.Models;
using System.Collections.Generic;

namespace EdFi.Ods.Security.Metadata.Contexts
{
    internal class SecurityContextSeed
    {
        private readonly SeedData _seedData = new SeedData();

        internal void SeedActions(SecurityContext context)
        {
            if (context.Actions.Any())
                return;
            
            context.Actions.AddRange(_seedData.Actions);
            context.SaveChanges();
        }

        internal void SeedApplication(SecurityContext context)
        {
            if (context.Applications.Any())
                return;

            context.Applications.Add(_seedData.OdsApplication);
            context.SaveChanges();
        }

        internal void SeedAuthorizationStrategies(SecurityContext context)
        {
            if (context.AuthorizationStrategies.Any())
                return;

            context.AuthorizationStrategies.AddRange(_seedData.AuthorizationStrategies);
            context.SaveChanges();
        }

        internal void SeedClaimSets(SecurityContext context)
        {
            if (context.ClaimSets.Any())
                return;

            context.ClaimSets.AddRange(_seedData.ClaimSets);
            context.SaveChanges();
        }

        internal void SeedResourceClaims(SecurityContext context)
        {
            if (context.ResourceClaims.Any())
                return;

            context.ResourceClaims.AddRange(_seedData.ResourceClaims);
            context.SaveChanges();
        }

        internal void SeedClaimSetResourceClaims(SecurityContext context)
        {
            if (context.ClaimSetResourceClaims.Any())
                return;

            context.ClaimSetResourceClaims.AddRange(_seedData.ClaimSetResourceClaims);
            context.SaveChanges();
        }

        internal void SeedResourceClaimsAuthorizationStrategies(SecurityContext context)
        {
            if (context.ResourceClaimAuthorizationStrategies.Any())
                return;

            context.ResourceClaimAuthorizationStrategies.AddRange(_seedData.ResourceClaimAuthorizationStrategies);
            context.SaveChanges();
        }
    }
}
