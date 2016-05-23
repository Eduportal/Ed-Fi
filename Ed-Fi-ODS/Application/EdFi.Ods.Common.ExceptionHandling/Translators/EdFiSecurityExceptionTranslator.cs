using System;
using System.Net;
using EdFi.Common.Security;

namespace EdFi.Ods.Common.ExceptionHandling.Translators
{
    public class EdFiSecurityExceptionTranslator : IExceptionTranslator
    {
        public bool TryTranslateMessage(Exception ex, out RESTError webServiceError)
        {
            webServiceError = null;

            if (ex is EdFiSecurityException)
            {
                webServiceError = new RESTError
                {
                    Code = (int)HttpStatusCode.Forbidden,
                    Type = "Forbidden",
                    Message = ex.GetAllMessages(),
                };

                return true;
            }

            return false;
        }
    }
}
