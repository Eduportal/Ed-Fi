using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using log4net;
using Newtonsoft.Json;
using RestSharp;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public class RestClientLoggingInterceptor : IInterceptor
    {
        private ILog _logger = LogManager.GetLogger(typeof(RestClientLoggingInterceptor));

        public void Intercept(IInvocation invocation)
        {
            var parameters = invocation.Method.GetParameters().Select((p, i) => Tuple.Create(p.Name, i));
            var requestParameter = parameters.SingleOrDefault(p => p.Item1 == "request");

            if (requestParameter == null || !_logger.IsDebugEnabled)
            {
                invocation.Proceed();
                return;
            }

            // Debugging enabled, and we're looking at a method with a "request" parameter
            var client = invocation.InvocationTarget as IRestClient;
            var request = invocation.Arguments[requestParameter.Item2] as IRestRequest;

            if (client == null)
            {
                _logger.DebugFormat("Unable to cast client to IRestClient. Unable to log information.");
            }
            else if (request != null)
            {
                // Find the JSON payload
                var bodyParameter = request.Parameters.SingleOrDefault(p => p.Type == ParameterType.RequestBody);

                var requestUri = client.BuildUri(request);

                _logger.DebugFormat(
                    "RestClient '{0}' method called using HTTP method '{1}' on resource URL '{2}':\r\n{3}",
                    invocation.Method.Name, request.Method, requestUri,
                    bodyParameter == null ? "(No body)" : bodyParameter.Value);

                invocation.Proceed();

                var response = invocation.ReturnValue as IRestResponse;

                if (response == null)
                    _logger.DebugFormat("Response from '{0}' was not an IRestReponse or was null.", requestUri);
                else
                    _logger.DebugFormat("Response from '{0}' returned status '{1}':\r\n{2}", 
                        requestUri, response.StatusCode, response.Content);

                return;
            }

            invocation.Proceed();
        }
    }
}
