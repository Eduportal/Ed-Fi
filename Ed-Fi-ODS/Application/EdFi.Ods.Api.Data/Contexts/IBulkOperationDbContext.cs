using System.Collections.Generic;
using System.Data.Entity;
using EdFi.Ods.Api.Data;
using EdFi.Ods.Api.Data.Model;

namespace EdFi.Ods.Api.Data.Contexts
{
    public interface IBulkOperationDbContext : IDbContext
    {
        IDbSet<BulkOperation> BulkOperations { get; set; }
        IDbSet<UploadFile> UploadFiles { get; set; }
        IDbSet<BulkOperationException> BulkOperationExceptions { get; set; }

        List<UploadFileChunkInfo> GetFileChunkInfos(string uploadFileId);
        void DeleteUploadFileChunks(string uploadFileId);

        string ConnectionString { get; }
    }
}