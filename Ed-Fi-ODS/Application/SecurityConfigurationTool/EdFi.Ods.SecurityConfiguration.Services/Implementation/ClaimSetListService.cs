using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.Security.Metadata.Contexts;
using EdFi.Ods.SecurityConfiguration.Services.Model;

namespace EdFi.Ods.SecurityConfiguration.Services.Implementation
{
    public class ClaimSetListService : IClaimSetListService
    {
        public IEnumerable<ClaimSet> GetAllClaimSets()
        {
            using (var context = new SecurityContext())
            {
                return context.ClaimSets.Select(c => new ClaimSet
                {
                    ClaimSetId = c.ClaimSetId,
                    ClaimSetName = c.ClaimSetName
                }).ToList();
            }
        }
    }
}