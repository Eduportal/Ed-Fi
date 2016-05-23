namespace EdFi.Ods.Tests.TestObjects
{
    using System.Linq;

    using global::EdFi.Ods.Api.Data;

    public class TestUploadFileChunkSet : TestDbSet<UploadFileChunk>
    {
        public override UploadFileChunk Find(params object[] keyValues)
        {
            var id = (string) keyValues.Single(k => k is string);
            var offset = (long) keyValues.Single(k => k is long);
            return this.SingleOrDefault(u => u.Id == id && u.Offset == offset);
        }
    }
}