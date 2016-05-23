using System;
using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.Common;
using EdFi.Ods.Common.ExceptionHandling;
using EdFi.Ods.Common.Utils;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Data;
using log4net;

namespace EdFi.Ods.BulkLoad.Core.Data
{
    public class PersistBulkOperationExceptions : IPersistBulkOperationExceptions
    {
        private readonly Func<IBulkOperationDbContext> _createContext;
        private readonly IRESTErrorProvider _errorProvider;
        private readonly ILog _log = LogManager.GetLogger(typeof(PersistBulkOperationExceptions));

        public PersistBulkOperationExceptions(Func<IBulkOperationDbContext> createContext, IRESTErrorProvider errorProvider)
        {
            _createContext = createContext;
            _errorProvider = errorProvider;
        }

        public void HandleFileValidationExceptions(string fileId, int exceptionCode, IEnumerable<string> messages)
        {
            using (var context = _createContext())
            {
                int i = 0;
                messages = messages.Take(500).ToList();
                foreach (var message in messages)
                {
                    _log.Error(string.Format("File {0} was invalid: {1} {2}", fileId, exceptionCode, message));
                    context.BulkOperationExceptions.Add(new BulkOperationException
                                                            {
                                                                Code = exceptionCode,
                                                                Message = message,
                                                                DateTime = SystemClock.Now(),
                                                                ParentUploadFileId = fileId
                                                            });
                    if (i % 10 == 0)
                        context.SaveChanges();
                    i++;
                }
                context.SaveChanges();
            }
        }

        public void HandleFileLoadingExceptions(string uploadFileId, LoadException[] exceptions)
        {
            using (var context = _createContext())
            {
                int i = 0;
                exceptions = exceptions.Take(500).ToArray();
                foreach (var ex in exceptions)
                {
                    _log.Error(ex.Exception.GetAllMessages() + Environment.NewLine + ex.Exception);
                    var wsError = _errorProvider.GetRestErrorFromException(ex.Exception);
                    if (string.IsNullOrWhiteSpace(wsError.Message))
                        return;
                    context.BulkOperationExceptions.Add(new BulkOperationException
                                                            {
                                                                Element = ex.Element,
                                                                Message = wsError.Message,
                                                                Code = wsError.Code,
                                                                Type = wsError.Type,
                                                                ParentUploadFileId = uploadFileId,
                                                                DateTime = DateTime.Now,
                                                                StackTrace = wsError.Type == "Internal Server Error" ? ex.Exception.GetAllStackTraces() : null
                                                            });
                    if (i % 10 == 0)
                        context.SaveChanges();
                    i++;
                }
                context.SaveChanges();
            }
        }

    }
}