using EdFi.Ods.Api.Data;

namespace EdFi.Ods.BulkLoad.Core.Data
{
    public interface IFindBulkOperationExceptions
    {
        BulkOperationException[] FindByUploadFile(string uploadFileId);
    }
}