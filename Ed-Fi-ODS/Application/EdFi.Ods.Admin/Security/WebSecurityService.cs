using WebMatrix.WebData;

namespace EdFi.Ods.Admin.Security
{
    /// <summary>
    /// Starting this class so that we can get a wrapper around WebSecurity and start to unit test the things that use it.
    /// </summary>
    public class WebSecurityService
    {
        public static void UpdatePasswordAndActivate(string username, string password)
        {
            var token = WebSecurity.GeneratePasswordResetToken(username);
            WebSecurity.ResetPassword(token, password);
        }
    }
}