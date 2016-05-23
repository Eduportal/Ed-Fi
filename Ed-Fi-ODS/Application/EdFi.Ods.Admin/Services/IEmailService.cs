namespace EdFi.Ods.Admin.Services
{
    public interface IEmailService
    {
        void SendConfirmationEmail(string emailAddress, string secret);
        void SendForgotPasswordEmail(string emailAddress, string secret);
    }
}