using System.Web;
using System.Web.Mvc;

namespace EdFi.Ods.Admin.Services
{
    using EdFi.Ods.Api.Common;

    public class RouteService : IRouteService
    {
        private UrlHelper Url { get { return new UrlHelper(HttpContext.Current.Request.RequestContext); } }

        public string GetRouteForPasswordReset(string marker)
        {
            return Url.Action("ResetPassword", "Account", new { Marker = marker })
                .ToAbsolutePath();
        }

        public string GetRouteForActivation(string marker)
        {
            return Url.Action("ActivateAccount", "Account", new { Marker = marker })
                .ToAbsolutePath();
        }
    }
}