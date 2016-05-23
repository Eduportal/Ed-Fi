using System.Web.Mvc;

namespace EdFi.Ods.Admin.Security
{
    public class RequireAuthenticatedUserAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!ValidateCurrentUser(filterContext))
            {
                var redirectUrl = new UrlHelper(filterContext.RequestContext).Action("Index", "Home");
                filterContext.Result = new RedirectResult(redirectUrl);
            }
        }

        public static bool ValidateCurrentUser(AuthorizationContext filterContext)
        {
            var httpContext = filterContext.RequestContext.HttpContext;
            return ValidateCurrentUser();
        }

        private static bool ValidateCurrentUser()
        {
            //TODO:  Make sure there is a currently authenticated user.
            return true;
        }
    }
}