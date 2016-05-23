using System.Collections.Generic;
using EdFi.Ods.SecurityConfiguration.Services.Model;

namespace EdFi.Ods.SecurityConfiguration.Services
{
    public interface IClaimSetListService
    {
        IEnumerable<ClaimSet> GetAllClaimSets();
    }
}