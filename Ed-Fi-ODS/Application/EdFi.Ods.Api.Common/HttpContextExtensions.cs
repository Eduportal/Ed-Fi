namespace EdFi.Ods.Api.Common
{
    using System;
    using System.Configuration;
    using System.Web;

    public static class HttpContextExtensions
    {
        public static string DeployedHostNameKey = "DeployedHostName";

        public static string ToAbsolutePath(this string path, string hashFragment = null)
        {
            var request = HttpContext.Current.Request;
            var requestUrl = request.Url;
            
            var scheme = requestUrl.Scheme;
            var host = ConfigurationManager.AppSettings[DeployedHostNameKey] ?? requestUrl.Host;
            var port = requestUrl.Port;
            var withApplicationPath = path.Replace("~", request.ApplicationPath).TrimEnd('/');
            var pathAndQuery = withApplicationPath.Split(new []{'?'}, 2);
            var withoutQuery = pathAndQuery[0];
            var query = pathAndQuery.Length > 1 ? pathAndQuery[1] : string.Empty;
            
            var builder = new UriBuilder(scheme, host, port)
            {
                Path = withoutQuery,
                Query = query
            };
            if (!string.IsNullOrWhiteSpace(hashFragment))
                builder.Fragment = hashFragment;

            var uri = builder.Uri;
            var absolutePath = uri.ToString();
            return absolutePath.TrimEnd('/');
        }
    }
}