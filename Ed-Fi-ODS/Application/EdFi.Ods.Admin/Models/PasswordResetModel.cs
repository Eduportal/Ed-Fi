namespace EdFi.Ods.Admin.Models
{
    public class ResetRequestValidationModel
    {
        public string UserName { get; set; }
        public string Marker { get; set; }
    }

    public class PasswordResetModel : ResetRequestValidationModel
    {
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}