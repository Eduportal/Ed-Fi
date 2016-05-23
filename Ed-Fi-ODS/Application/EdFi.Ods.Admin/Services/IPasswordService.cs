using EdFi.Ods.Admin.Models;
using EdFi.Ods.Admin.Models.Results;

namespace EdFi.Ods.Admin.Services
{
    public interface IPasswordService
    {
        PasswordResetResult ValidateRequest(string userName, string secret);
        ConfirmationResult ConfirmAccount(string secret);
        string SetPasswordResetSecret(string userName);
        bool PasswordIsStrong(string password);
        User GetUserForPasswordResetToken(string secret);
    }
}