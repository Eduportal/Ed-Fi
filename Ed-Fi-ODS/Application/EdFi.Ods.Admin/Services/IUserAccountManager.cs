using EdFi.Ods.Admin.Models;
using EdFi.Ods.Admin.Models.Results;

namespace EdFi.Ods.Admin.Services
{
    public interface IUserAccountManager
    {
        CreateLoginResult Create(CreateLoginModel model);
        PasswordResetResult ResetPassword(PasswordResetModel model);
        ChangePasswordResult ChangePassword(ChangePasswordModel model);
        ForgotPasswordResetResult ForgotPassword(ForgotPasswordModel model);
        ForgotPasswordResetResult ResendConfirmation(ForgotPasswordModel model);
    }
}