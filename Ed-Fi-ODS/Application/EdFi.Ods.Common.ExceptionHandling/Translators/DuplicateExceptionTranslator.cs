using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NHibernate;

namespace EdFi.Ods.Common.ExceptionHandling.Translators
{
    public class DuplicateExceptionTranslator : IExceptionTranslator
    {
        private const string ExpectedExceptionPattern =
        @"^a different object with the same identifier value was already associated with the session: (?<subject>\w*): (?<subjectId>\d*), (?<entitySimple>\w*): (?<property>\w*): (?<entityPropertyId>\d*), of entity: (?<entityFullName>\w*)";
        public bool TryTranslateMessage(Exception ex, out RESTError webServiceError)
        {
            webServiceError = null;
            if (ex is NonUniqueObjectException)
            {
                var match = Regex.Match(ex.Message, ExpectedExceptionPattern);
                if (match.Success)
                {
                    try
                    {
                        webServiceError = new RESTError
                        {
                            Code = (int)HttpStatusCode.Conflict,
                            Type = HttpStatusCode.Conflict.ToString(),
                            Message = string.Format("A duplicate {0} conflict occurred when attempting to create a new {1} resource with {2} of {3}.",
                            match.Groups["subject"].Value,
                            match.Groups["entitySimple"].Value,
                            match.Groups["property"].Value,
                            match.Groups["entityPropertyId"].Value)
                        };
                    }
                    catch
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
