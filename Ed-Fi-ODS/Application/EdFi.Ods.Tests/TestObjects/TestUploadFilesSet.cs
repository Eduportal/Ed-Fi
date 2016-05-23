namespace EdFi.Ods.Tests.TestObjects
{
    using System.Linq;

    using global::EdFi.Ods.Api.Data;

    public class TestUploadFilesSet : TestDbSet<UploadFile>
    {
        public override UploadFile Find(params object[] keyValues)
        {
            var id = (string) keyValues.Single();
            return this.SingleOrDefault(u => u.Id == id);
        }
    }
}