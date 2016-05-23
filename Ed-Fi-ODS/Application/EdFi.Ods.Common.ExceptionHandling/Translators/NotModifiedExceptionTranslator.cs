using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EdFi.Ods.Common.Exceptions;
using EdFi.Common.Security;

namespace EdFi.Ods.Common.ExceptionHandling.Translators
{
    public class NotModifiedExceptionTranslator : IExceptionTranslator
    {
        public bool TryTranslateMessage(Exception ex, out RESTError webServiceError)
        {
            webServiceError = null;

            if (ex is NotModifiedException)
            {
                webServiceError = new RESTError
                {
                    Code = (int)HttpStatusCode.NotModified,
                    Type = "Not Modified"
                };

                return true;
            }

            return false;
        }
    }
}
