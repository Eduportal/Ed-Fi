using System.Collections.Generic;
using EdFi.Ods.Api.Data;

namespace EdFi.Ods.BulkLoad.Core
{
    public interface IPersistUploadFiles
    {
        UploadFilePersistenceResult Persist(string operationId, string uploadFileId);
    }
}