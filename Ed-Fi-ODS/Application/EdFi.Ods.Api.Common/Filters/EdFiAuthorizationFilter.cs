using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Security.Metadata.Repositories;
using System.Security.Claims;

namespace EdFi.Ods.Api.Common.Filters
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class EdFiAuthorizationAttribute : Attribute
    {
        public string Resource { get; set; }
    }

    /// <summary>
    /// This class is intended for use only with the EdFi.Identity.IdentitiesController
    /// For Identity we don't need to populate an EdFiAuthorizationContextData object and can use the Principal set on the rquest context.
    /// Use of this filter with EdFi.ODS.Api.Controllers will not work with current implementation.
    /// </summary>
    public class EdFiAuthorizationFilter : IAuthorizationFilter
    {
        private readonly ISecurityRepository _securityRepository;
        private readonly IEdFiAuthorizationProvider _authorizationProvider;
        public EdFiAuthorizationFilter(IEdFiAuthorizationProvider authorizationProvider, ISecurityRepository securityRepository)
        {
            _authorizationProvider = authorizationProvider;
            _securityRepository = securityRepository;
        }

        public Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            var actionAttribute = actionContext.ActionDescriptor.GetCustomAttributes<EdFiAuthorizationAttribute>().SingleOrDefault();
            var controllerAttribute = actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<EdFiAuthorizationAttribute>().SingleOrDefault();

            var authorizationAttribute = actionAttribute ?? controllerAttribute;
            
            if (authorizationAttribute == null)
                return continuation();

            _authorizationProvider.AuthorizeSingleItem(CreateAuthorizationContext(actionContext, authorizationAttribute));

            return continuation();
        }

        public bool AllowMultiple { get { return false; } }

        private EdFiAuthorizationContext CreateAuthorizationContext(HttpActionContext actionContext, EdFiAuthorizationAttribute authorizationAttribute)
        {
            return new EdFiAuthorizationContext((ClaimsPrincipal)actionContext.RequestContext.Principal, 
                                                authorizationAttribute.Resource, 
                                                _securityRepository.GetActionByHttpVerb(actionContext.Request.Method.Method).ActionUri, 
                                                null as object);
        }
    }
}
