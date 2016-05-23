using EdFi.Ods.Api.Data;

namespace EdFi.Ods.BulkLoad.Core.Data
{
    public interface IFindBulkOperations
    {
        BulkOperation FindAndStart(string id);
        BulkOperation FindWithFiles(string id);
        BulkOperation FindWithoutFiles(string id);
    }
}