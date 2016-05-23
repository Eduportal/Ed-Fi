using EdFi.Ods.Admin.Models;

namespace EdFi.Ods.Admin.Services
{
    public interface IDefaultApplicationCreator
    {
        Application FindOrCreateUpdatedDefaultSandboxApplication(int vendorId, SandboxType sandboxType);
    }
}