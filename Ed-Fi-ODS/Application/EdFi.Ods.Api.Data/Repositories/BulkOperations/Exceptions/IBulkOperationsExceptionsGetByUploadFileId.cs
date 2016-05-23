using EdFi.Ods.Api.Common;
using EdFi.Ods.Api.Models.Resources;

namespace EdFi.Ods.Api.Data.Repositories.BulkOperations.Exceptions
{
    public interface IBulkOperationsExceptionsGetByUploadFileId
    {
        Models.Resources.BulkOperationException[] GetByUploadFileId(string id, QueryParameters parameters);
    }
}