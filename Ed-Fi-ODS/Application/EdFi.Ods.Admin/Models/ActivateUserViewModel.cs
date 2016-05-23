namespace EdFi.Ods.Admin.Models
{    
    public class ActivateUserViewModel
    {
        public string Marker { get; set; }
        public string UserName { get; set; }
        public string ErrorMessage { get; set; }
        public bool ShowErrorMessage { get { return !string.IsNullOrWhiteSpace(ErrorMessage); } }
        public bool ShowResendEmailLink { get; set; }
    }
}