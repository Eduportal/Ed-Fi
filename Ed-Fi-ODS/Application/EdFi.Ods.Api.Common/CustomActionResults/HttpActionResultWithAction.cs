using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace EdFi.Ods.Api.Common.CustomActionResults
{
    public class HttpActionResultWithAction : IHttpActionResult
    {
        private IHttpActionResult _innerResult;
        private string _responsePhrase;
        private Action<HttpResponseMessage> _responseAction;

        public HttpActionResultWithAction(IHttpActionResult inner, Action<HttpResponseMessage> responseAction)
            : this(inner, null, responseAction)
        {
        }

        public HttpActionResultWithAction(IHttpActionResult inner, string responsePhrase, Action<HttpResponseMessage> responseAction = null)
        {
            _innerResult = inner;
            _responsePhrase = responsePhrase;
            _responseAction = responseAction;
        }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = await _innerResult.ExecuteAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(_responsePhrase))
            {
                response.ReasonPhrase = _responsePhrase;
            }
            if (_responseAction != null)
            {
                _responseAction(response);
            }
            return response;
        }
    }
}