using EdFi.Ods.Api.Data;

namespace EdFi.Ods.BulkLoad.Console
{
    public interface IBulkExecutor
    {
        bool Execute(string operationId);
    }
}