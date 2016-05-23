using System.Collections.Generic;
using System.Linq;

namespace EdFi.Ods.BulkLoad.Console
{
    public class LocalBulkOperation
    {
        public static readonly LocalBulkOperation Empty = new LocalBulkOperation(null, new LocalUploadFile[0]);

        public LocalBulkOperation(string operationId, IEnumerable<LocalUploadFile> localUploadFiles)
        {
            OperationId = operationId;
            LocalUploadFiles = localUploadFiles.ToArray();
        }

        public string OperationId { get; private set; }
        public LocalUploadFile[] LocalUploadFiles { get; private set; }

        public bool IsEmpty { get { return LocalUploadFiles.Length == 0; } }
    }
}