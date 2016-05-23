using System.Web.Http;
using EdFi.Ods.Api.Common.CustomActionResults;

namespace EdFi.Ods.Api.Architecture
{
    public static class ProfileHttpResultExtensions
    {
        /// <sumary>
        /// Replaces the content type header on the supplied <see cref="IHttpActionResult"/>
        /// with the supplied content type.
        public static IHttpActionResult WithContentType(this IHttpActionResult result,
                                                        string contentType)
        {
            return result.With(
                x =>
                {
                    x.Content.Headers.Remove("Content-Type");
                    x.Content.Headers.Add("Content-Type", contentType);
                });
        }
    }
}