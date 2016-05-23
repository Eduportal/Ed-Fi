using System.Collections.Generic;

namespace EdFi.Ods.BulkLoad.Core.Data
{
    public interface IPersistBulkOperationExceptions
    {
        void HandleFileValidationExceptions(string fileId, int exceptionCode, IEnumerable<string> messages);
        void HandleFileLoadingExceptions(string uploadFileId, LoadException[] exceptions);
    }
}