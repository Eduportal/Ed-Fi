namespace EdFi.Ods.Tests.EdFi.Ods.WebApi.Common
{
    using System;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Hosting;
    using System.Web.Http.Routing;

    public class RouteTester
    {
        HttpConfiguration config;
        HttpRequestMessage request;
        IHttpRouteData routeData;
        IHttpControllerSelector controllerSelector;
        HttpControllerContext controllerContext;

        public RouteTester(HttpConfiguration conf)
        {
            this.config = conf;
        }

        public string ValidateRoutingForUrl(string method, string url, Type controller, string action)
        {
            this.request = new HttpRequestMessage(this.GetMethod(method), url);
            this.routeData = this.config.Routes.GetRouteData(this.request);
            if (this.routeData == null) return url + " failed to resolve to any controller";
            this.request.Properties[HttpPropertyKeys.HttpRouteDataKey] = this.routeData;
            this.controllerSelector = new DefaultHttpControllerSelector(this.config);
            this.controllerContext = new HttpControllerContext(this.config, this.routeData, this.request);
            var resolvedController = this.GetControllerType();
            if (controller != resolvedController)
            {
                return url + " resolved to " + resolvedController + " instead of " + controller.Name;
            }
            else
            {
                var resolvedAction = this.GetActionName();
                if (action != resolvedAction)
                {
                    return url + " resolved to " + resolvedAction + " instead of " + action;
                }
            }
            return string.Empty;
        }

        private HttpMethod GetMethod(string method)
        {
            if (method.Equals("GET")) return HttpMethod.Get;
            if (method.Equals("POST")) return HttpMethod.Post;
            return HttpMethod.Get;//default
        }

        private Type GetControllerType()
        {
            try
            {
                var descriptor = this.controllerSelector.SelectController(this.request);
                this.controllerContext.ControllerDescriptor = descriptor;
                return descriptor.ControllerType;
            }
            catch (Exception exception)
            {
                var s = exception.Message;
                return typeof(object);
            }
        }

        private string GetActionName()
        {
            try
            {
                if (this.controllerContext.ControllerDescriptor == null)
                    this.GetControllerType();
                var actionSelector = new ApiControllerActionSelector();
                var descriptor = actionSelector.SelectAction(this.controllerContext);
                return descriptor.ActionName;
            }
            catch (Exception)
            {
                return "<Null>";
            }
        }
    }
}
