
namespace EdFi.Ods.Api.Data.Repositories.BulkOperations
{
    public interface IBulkOperationsFileChunkCreator
    {
        string CreateChunk(string uploadFileId, long offset, long size);
        bool VerifyChunkSize(string id, long size);
    }
}
