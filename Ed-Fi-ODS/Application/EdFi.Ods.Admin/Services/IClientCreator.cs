using EdFi.Ods.Admin.Models;
using EdFi.Ods.Admin.Models.Client;

namespace EdFi.Ods.Admin.Services
{
    public interface IClientCreator
    {
        ApiClient CreateNewSandboxClient(SandboxClientCreateModel createModel, User user);
    }
}