
namespace EdFi.Ods.Api.Data.Repositories.BulkOperations
{
    public interface IVarbinaryWriter
    {
        void Write(string id, byte[] bytes, int offset, int count);
    }
}