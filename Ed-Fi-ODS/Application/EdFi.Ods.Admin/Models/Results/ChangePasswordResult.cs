namespace EdFi.Ods.Admin.Models.Results
{
    public class ChangePasswordResult : AdminActionResult<ChangePasswordResult, ChangePasswordModel>
    {
        public static ChangePasswordResult Successful = new ChangePasswordResult {Success = true};
    }
}