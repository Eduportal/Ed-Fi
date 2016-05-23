namespace EdFi.Ods.BulkLoad.Core.Data
{
    public interface IDeleteUploadFileChunks
    {
        void DeleteByUploadFileId(string uploadFileId);
    }
}
