using System;
using System.Collections.Generic;
using System.Net;

namespace EdFi.Ods.Common.ExceptionHandling
{
    public interface IRESTErrorProvider
    {
        RESTError GetRestErrorFromException(Exception exception);
    }

    public class RESTErrorProvider : IRESTErrorProvider
    {
        private readonly IEnumerable<IExceptionTranslator> _translators;

        public RESTErrorProvider(IEnumerable<IExceptionTranslator> translators)
        {
            this._translators = translators;
        }

        public RESTError GetRestErrorFromException(Exception exception)
        {
            // Try to translate the exception explicitly
            foreach (var translator in _translators)
            {
                RESTError error;

                if (translator.TryTranslateMessage(exception, out error))
                    return error;
            }
            
            // Default exception message is to just return all the messages in the exception stack as an Internal Server Error
            var response = new RESTError
            {
                // This class translates into a serialized output that matches inBloom's approach to error handling.
                Code = (int) HttpStatusCode.InternalServerError,
                Type = "Internal Server Error",
                Message = "An unexpected error occurred on the server."
            };

            return response;
        }
    }
}