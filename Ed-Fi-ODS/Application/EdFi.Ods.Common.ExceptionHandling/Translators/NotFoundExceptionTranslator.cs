using System;
using System.Net;
using EdFi.Ods.Common.Exceptions;

namespace EdFi.Ods.Common.ExceptionHandling.Translators
{
    public class NotFoundExceptionTranslator : IExceptionTranslator
    {
        public bool TryTranslateMessage(Exception ex, out RESTError webServiceError)
        {
            webServiceError = null;

            if (ex is NotFoundException)
            {
                webServiceError = new RESTError
                {
                    Code = (int)HttpStatusCode.NotFound,
                    Type = "Not Found",
                    Message = ex.GetAllMessages()?? "The specified resource could not be found."
                };

                return true;
            }

            return false;
        }
    }
}
