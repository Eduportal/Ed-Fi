using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace EdFi.Ods.Api.Common.CustomActionResults
{
    public class HttpActionResultWithError : IHttpActionResult
    {
        private IHttpActionResult _innerResult;
        private string _errorMessage;
        private Action<HttpResponseMessage> _responseAction;

        public HttpActionResultWithError(IHttpActionResult inner, Action<HttpResponseMessage> responseAction)
            : this(inner, null, responseAction)
        {
        }

        public HttpActionResultWithError(IHttpActionResult inner, string errorMessage, Action<HttpResponseMessage> responseAction = null)
        {
            _innerResult = inner;
            _errorMessage = errorMessage;
            _responseAction = responseAction;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = await _innerResult.ExecuteAsync(cancellationToken);
            var retvalue = response.RequestMessage.CreateErrorResponse(response.StatusCode, _errorMessage);
            if (_responseAction != null)
            {
                _responseAction(retvalue);
            }
            return retvalue;
        }
    }
}