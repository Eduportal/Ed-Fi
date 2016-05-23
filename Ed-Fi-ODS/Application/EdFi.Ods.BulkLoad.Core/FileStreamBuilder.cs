using System.IO;

namespace EdFi.Ods.BulkLoad.Core
{
    public class FileStreamBuilder : IStreamBuilder
    {
        public Stream Build(string source)
        {
            return new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
    }
}