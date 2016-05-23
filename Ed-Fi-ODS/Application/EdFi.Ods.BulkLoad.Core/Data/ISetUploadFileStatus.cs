using EdFi.Ods.Api.Models.Resources.Enums;

namespace EdFi.Ods.BulkLoad.Core.Data
{
    public interface ISetUploadFileStatus
    {
        void SetStatus(string fileId, UploadFileStatus status);
    }
}