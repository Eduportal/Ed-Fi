using System;
using System.Web;

namespace EdFi.Ods.SecurityConfiguration.KeyRetrieval.Web
{
    public class GlobalApplication : HttpApplication
    {
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var request = HttpContext.Current.Request;

            // if deployed in a sub-folder, the root should end with slash. 
            if (request.FilePath != "/" && request.ApplicationPath == request.FilePath)
            {
                Response.Redirect(request.Path + "/");
            }
        }
    }
}
