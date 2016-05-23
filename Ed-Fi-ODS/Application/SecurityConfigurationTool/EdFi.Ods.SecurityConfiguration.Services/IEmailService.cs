using EdFi.Ods.SecurityConfiguration.Services.Model;

namespace EdFi.Ods.SecurityConfiguration.Services
{
    public interface IEmailService
    {
        void SendActivationRequestEmail(EmailParameters parameters);
    }
}