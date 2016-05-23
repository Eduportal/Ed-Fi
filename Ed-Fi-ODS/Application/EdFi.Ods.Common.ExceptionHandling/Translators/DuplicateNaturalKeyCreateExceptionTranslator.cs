using System;
using System.Data.SqlClient;
using System.Net;
using System.Text.RegularExpressions;
using NHibernate.Exceptions;

namespace EdFi.Ods.Common.ExceptionHandling.Translators
{
    public class DuplicateNaturalKeyCreateExceptionTranslator : IExceptionTranslator
    {
        private const string ExpectedExceptionPattern =
            @"^Violation of PRIMARY KEY constraint '[^\s]+'\.\s+Cannot insert duplicate key in object '[^\s]+'\.\s+The duplicate key value is \(.*\)\.\s+The statement has been terminated\.\s*$";

        public bool TryTranslateMessage(Exception ex, out RESTError webServiceError)
        {
            webServiceError = null;
            if (ex is GenericADOException)
            {
                if (ex.InnerException is SqlException)
                {
                    var innerExceptionMessage = ex.InnerException.Message;
                    if (Regex.IsMatch(innerExceptionMessage, ExpectedExceptionPattern))
                    {
                        webServiceError = new RESTError
                                              {
                                                  Code = (int) HttpStatusCode.Conflict,
                                                  Type = HttpStatusCode.Conflict.ToString(),
                                                  Message =
                                                      "A natural key conflict occurred when attempting to create a new resource with a duplicate key. This is likely caused by multiple resources with the same key in the same file. Exactly one of these resources was inserted."
                                              };
                        return true;
                    }
                }
            }
            return false;
        }
    }
}