using System;
using System.Web.Mvc;
using EdFi.Ods.Admin.Services;

namespace EdFi.Ods.Admin.Filters
{
    public class SetCurrentUserInfoAttribute : ActionFilterAttribute
    {
        private readonly Func<ISecurityService> _securityServiceLocator;

        //NOTE:  This uses a Func to supply the security service rather than normal constructor injection.  This allows
        //       us to create the filter during App_Start, but wait until the request cycle to resolve the dependencies.
        //       This allows us to use LifecyclePerWebRequest with the Castle container.  Castle can't handle resolving
        //       PerWebRequest dependencies during App_Start.

        public SetCurrentUserInfoAttribute(Func<ISecurityService> securityServiceLocator)
        {
            _securityServiceLocator = securityServiceLocator;
        }

        private ISecurityService SecurityService
        {
            get { return _securityServiceLocator(); }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
                return;

            var userLookup = SecurityService.GetCurrentUser();
            filterContext.Controller.ViewBag.UserLookup = userLookup;
        }
    }
}