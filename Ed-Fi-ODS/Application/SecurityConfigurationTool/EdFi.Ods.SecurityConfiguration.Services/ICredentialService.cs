using EdFi.Ods.SecurityConfiguration.Services.Model;

namespace EdFi.Ods.SecurityConfiguration.Services
{
    public interface ICredentialService
    {
        Credentials GetNewCredentials();
    }
}