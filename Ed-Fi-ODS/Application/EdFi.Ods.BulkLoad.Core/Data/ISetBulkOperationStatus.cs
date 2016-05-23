using EdFi.Ods.Api.Models.Resources.Enums;

namespace EdFi.Ods.BulkLoad.Core.Data
{
    public interface ISetBulkOperationStatus
    {
        void SetStatus(string id, BulkOperationStatus status);
    }
}