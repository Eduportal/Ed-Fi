using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace EdFi.Ods.WebService.Tests.Profiles
{
    public class SwaggerUITestStartup
    {
        public const string BaseUrl = "http://localhost:5555/";

        // Invoked once at startup to configure your application.
        public void Configuration(IAppBuilder app)
        {
            // Add "middleware" for handling the ashx request ourselves
            app.Use<AppSettingsMiddleware>();

            // Find the path to the SwaggerUI project
            string swaggerUIPath =
                (Directory.GetCurrentDirectory()
                          .Split(new[] {@"\Application\"}, StringSplitOptions.None)[0]
                 + @"\Application\EdFi.Ods.SwaggerUI\")
                    .Replace("Ed-Fi-ODS", "Ed-Fi-ODS-Implementation");

            // Serve up all the static files
            app.UseFileServer(
                new FileServerOptions()
                {
                    RequestPath = PathString.Empty,
                    FileSystem = new PhysicalFileSystem(swaggerUIPath),
                });
        }

        /// <summary>
        /// Enables a self-hosted OWIN-based SwaggerUI website to be launched by providing a direct 
        /// replacement of the behavior provided by the classic ASP.NET artifact "appSettings.ashx".
        /// </summary>
        /// <remarks>This middleware provides the necessary values to enable the SwaggerUI to find
        /// the Ed-Fi ODS API's metadata endpoints, behavior which is derived from the web.config 
        /// appSettings section in the out-of-the-box SwaggerUI website.</remarks>
        private class AppSettingsMiddleware : OwinMiddleware
        {
            private readonly OwinMiddleware _next;

            /// <summary>
            /// Instantiates the middleware with an optional pointer to the next component.
            /// </summary>
            /// <param name="next"/>
            public AppSettingsMiddleware(OwinMiddleware next)
                : base(next)
            {
                _next = next;
            }

            /// <summary>
            /// Process an individual request.
            /// </summary>
            /// <param name="context"/>
            /// <returns/>
            public override async Task Invoke(IOwinContext context)
            {
                //var environment = context.Environment;
                //string requestPath = (string)environment["owin.RequestPath"];

                string requestPath = context.Request.Path.ToString();

                if (requestPath == "/appSettings.ashx")
                {
                    await Task.Factory.StartNew(
                        () =>
                        {
                            var values = new Dictionary<string, string>
                            {
                                {"webApi",            "http://localhost:4444/api/v2.0/" },
                                {"webApiMetadataUrl", "http://localhost:4444/metadata/{section}/api-docs"},
                                {"adminUrl",          "http://localhost:9999/NO_ADMIN_SITE_DEPLOYED/oauth"},
                            };

                            var response = context.Response;

                            response.Write("var appSettings = {");

                            foreach (var kvp in values)
                                response.Write(string.Format("'{0}': '{1}',", kvp.Key, kvp.Value));

                            response.Write("'loaded': true };");

                            context.Response.ContentType = "text/javascript";
                        });
                }
                else
                {
                    await _next.Invoke(context);
                }
            }
        }
    }
}