using System;
using System.Net.Http;
using System.Web.Http;

namespace EdFi.Ods.Api.Common.CustomActionResults
{
    public static class HttpActionResultExtensions
    {
        public static IHttpActionResult With(this IHttpActionResult inner, Action<HttpResponseMessage> responseAction)
        {
            return new HttpActionResultWithAction(inner, responseAction);
        }

        public static IHttpActionResult WithError(this IHttpActionResult inner, string errorMessage)
        {
            return new HttpActionResultWithError(inner, errorMessage);
        }
    }
}