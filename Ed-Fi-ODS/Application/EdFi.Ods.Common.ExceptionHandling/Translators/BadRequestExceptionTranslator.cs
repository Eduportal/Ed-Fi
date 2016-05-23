using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;

namespace EdFi.Ods.Common.ExceptionHandling.Translators
{
    public class BadRequestExceptionTranslator : IExceptionTranslator
    {
        private readonly Type[] _badRequestExceptionTypes =
        {
            typeof (ArgumentException), typeof (ValidationException),
            typeof (FormatException)
        };

        public bool TryTranslateMessage(Exception ex, out RESTError webServiceError)
        {
            webServiceError = null;

            if (!_badRequestExceptionTypes.Contains(ex.GetType())) return false;

            webServiceError = new RESTError
            {
                Code = (int)HttpStatusCode.BadRequest,
                Type = "Bad Request",
                Message = ex.GetAllMessages(),
            };

            return true;
        }
    }
}
