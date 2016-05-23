namespace EdFi.Ods.Admin.Models.Results
{

    public class ConfirmationResult : AdminActionResult<ConfirmationResult, PasswordResetModel>
    {
        public static ConfirmationResult Successful(string userName, string passwordResetToken)
        {
            return new ConfirmationResult()
            {
                Success = true,
                UserName = userName,
                PasswordResetToken = passwordResetToken
            };
        }

        public static ConfirmationResult UserAlreadyConfirmed = new ConfirmationResult
            {
                Success = false,
                Message = "This account has already been confirmed.  Would you like to reset your password?",
            };

        public static ConfirmationResult Failure = new ConfirmationResult
            {
                Success = false,
                Message = "Your confirmation request is invalid or expired."
            };

        public string PasswordResetToken { get; private set; }
        public string UserName { get; set; }
    }
}