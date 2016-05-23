using EdFi.Ods.Admin.Models;

namespace EdFi.Ods.Admin.Services
{
    public interface IDatabaseTemplateLeaQuery
    {
        int[] GetLocalEducationAgencyIds(SandboxType sandboxType);
    }
}