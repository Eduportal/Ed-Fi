namespace EdFi.Ods.Admin.Models
{
    public class ActivateUserModel
    {
        public string Marker { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}