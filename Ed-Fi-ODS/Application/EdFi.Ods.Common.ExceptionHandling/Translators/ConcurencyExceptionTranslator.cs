using System;
using System.Net;
using EdFi.Ods.Common.Exceptions;

namespace EdFi.Ods.Common.ExceptionHandling.Translators
{
    public class ConcurencyExceptionTranslator : IExceptionTranslator
    {
        public bool TryTranslateMessage(Exception ex, out RESTError webServiceError)
        {
            webServiceError = null;

            if (ex is ConcurrencyException)
            {
                // See RFC 5789 - Conflicting modification (enforced internally, and no "If-Match" header)
                webServiceError = new RESTError
                {
                    Code = (int)HttpStatusCode.Conflict,
                    Type = "Conflict",
                    Message = ex.GetAllMessages(),
                };

                return true;
            }

            return false;
        }
    }
}
