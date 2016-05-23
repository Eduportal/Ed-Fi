using System;
using System.Net;
using NHibernate;

namespace EdFi.Ods.Common.ExceptionHandling.Translators
{
    public class DuplicateNaturalKeyUpdateExceptionTranslator : IExceptionTranslator
    {
        public bool TryTranslateMessage(Exception ex, out RESTError webServiceError)
        {
            webServiceError = null;
            if (ex is StaleObjectStateException)
            {
                webServiceError = new RESTError
                {
                    Code = (int)HttpStatusCode.Conflict,
                    Type = HttpStatusCode.Conflict.ToString(),
                    Message =
                        "A natural key conflict occurred when attempting to update a new resource with a duplicate key. This is likely caused by multiple resources with the same key in the same file. Exactly one of these resources was updated."
                };
                return true;
            }
            return false;
        }
    }
}