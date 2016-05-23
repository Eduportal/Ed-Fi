namespace EdFi.Ods.Admin.Models.Results
{
    public class ForgotPasswordResetResult : AdminActionResult<ForgotPasswordResetResult, ForgotPasswordModel>
    {
        public static ForgotPasswordResetResult Successful = new ForgotPasswordResetResult {Success = true};

        public static ForgotPasswordResetResult BadEmail(string email)
        {
            var message = string.Format("Could not locate an account with email address '{0}'.", email);
            return new ForgotPasswordResetResult {Success = false, Message = message};
        }
    }
}