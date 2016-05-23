namespace EdFi.Ods.Admin.Models.Results
{
    public class CreateLoginResult : AdminActionResult<CreateLoginResult, CreateLoginModel>
    {
        public static CreateLoginResult Fail
        {
            get { return new CreateLoginResult {UserStatus = UserStatus.Failed, Success = false}; }
        }

        public UserStatus UserStatus { get; set; }

        public string UserStatusMessage
        {
            get { return UserStatus.ToString(); }
        }
    }
}