using EdFi.Ods.Api.Data;

namespace EdFi.Ods.BulkLoad.Core.Data
{
    public interface ICreateBulkOperation
    {
        void Create(BulkOperation operation);
    }
}