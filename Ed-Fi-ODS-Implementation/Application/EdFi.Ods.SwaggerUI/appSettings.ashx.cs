using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace EdFi.Ods.SwaggerUI
{
    /// <summary>
    /// Summary description for EnvironmentToJavascript1
    /// </summary>
    /// <remarks>
    ///     Include a link to the dynamically generated script to the scripts section of the web page
    ///     <script src="appSettings.ashx" type="text/javascript"></script>
    /// </remarks>
    public class AppSettings : IHttpHandler
    {
        /// <summary>
        /// Create an "appSettings" namespace in a javascript file containing all the app settings from the web.config file
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/javascript";
            context.Response.Write("var appSettings = {");
            foreach (var key in ConfigurationManager.AppSettings.AllKeys.Where(x => x.StartsWith("swagger.")))
            {
                context.Response.Write(string.Format("'{0}': '{1}',", key.Substring("swagger.".Length), ConfigurationManager.AppSettings[key]));
            }
            context.Response.Write("'loaded': true };");
        }

        /// <summary>
        /// No dynamic values here, so feel free to cache the results. 
        /// If the web.config file changes, the app pool will also reset
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}