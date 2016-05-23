using System.IO;

namespace EdFi.Ods.BulkLoad.Core
{
    public interface IStreamBuilder
    {
        Stream Build(string source);
    }
}