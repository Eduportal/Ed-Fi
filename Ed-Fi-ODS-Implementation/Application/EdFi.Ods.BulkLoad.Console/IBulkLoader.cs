using EdFi.Ods.BulkLoad.Core;

namespace EdFi.Ods.BulkLoad.Console
{
    public interface IBulkLoaderOld
    {
        bool Load(BulkLoaderConfiguration config);
    }
}