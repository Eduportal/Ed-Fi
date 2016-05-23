using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using EdFi.Common;
using EdFi.Common.Extensions;
using NHibernate.Exceptions;


namespace EdFi.Ods.Common.ExceptionHandling.Translators
{
    public class SqlServerUniqueIndexExceptionTranslator : IExceptionTranslator
    {
        private readonly IDatabaseMetadataProvider databaseMetadataProvider;

        private static Regex expression = new Regex(@"^Cannot insert duplicate key row in object '[a-z]+\.(?<TableName>\w+)' with unique index '(?<IndexName>\w+)'|^Violation of UNIQUE KEY constraint '(?<IndexName>\w+)'. Cannot insert duplicate key in object '[a-z]+\.(?<TableName>\w+)'.");
        private static string singleMessageFormat = "The value supplied for property '{0}' of entity '{1}' is not unique.";
        private static string multipleMessageFormat = "The values supplied for properties '{0}' of entity '{1}' are not unique.";

        public SqlServerUniqueIndexExceptionTranslator(IDatabaseMetadataProvider databaseMetadataProvider)
        {
            this.databaseMetadataProvider = databaseMetadataProvider;
        }

        public bool TryTranslateMessage(Exception ex, out RESTError webServiceError)
        {
            webServiceError = null;

            var exception = (ex is GenericADOException) ? ex.InnerException : ex;

            if (exception is SqlException)
            {
                var match = expression.Match(exception.Message);

                if (match.Success)
                {
                    string indexName = match.Groups["IndexName"].Value;
                    var indexDetails = databaseMetadataProvider.GetIndexDetails(indexName);

                    string columnNames = indexDetails == null ?
                        "(unknown)" :
                        string.Join("', '", indexDetails.ColumnNames.Select(x => x.ToCamelCase()));

                    string message;

                    if (indexDetails.ColumnNames.Count == 1)
                    {
                        message = string.Format(singleMessageFormat, columnNames, indexDetails.TableName.ToCamelCase());
                    }
                    else
                    {
                        message = string.Format(multipleMessageFormat, columnNames, indexDetails.TableName.ToCamelCase());
                    }

                    webServiceError = new RESTError
                    {
                        Code = (int) HttpStatusCode.Conflict,
                        Type = "Conflict",
                        Message = message,
                    };

                    return true;
                }
            }

            return false;
        }
    }
}
