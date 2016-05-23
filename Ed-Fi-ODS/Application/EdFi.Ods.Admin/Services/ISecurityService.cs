namespace EdFi.Ods.Admin.Services
{
    public interface ISecurityService
    {
        UserLookupResult GetCurrentUser();
        UserIdLookupResult GetCurrentUserId();
    }
}